$(document).ready(function () {
    let currentVisitorId = null;
    let isEditMode = false;

    $('#detailsModal').on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget);
        var presenceId = button.data('presence-id');
        isEditMode = false;
        var modal = $(this);

        // Reset modal content mentre carica
        modal.find('.modal-body').html('Caricamento...');

        loadVisitorDetails(presenceId, modal);
    });

    function loadVisitorDetails(presenceId, modal) {
        $.getJSON('/Presence/DetailsJson', { presenceId: presenceId }, function (data) {
            currentVisitorId = data.visitorId; // Usa visitorId dalla risposta
            renderModalContent(data, false);
        });
    }

    function renderModalContent(data, editMode) {
        let qrHtml = data.qrCodeImageBase64
            ? `<img src="data:image/png;base64,${data.qrCodeImageBase64}" style="max-width:200px;" />
               <br />
               <code>${data.qrCode}</code>`
            : `<em>QR code non disponibile</em>`;

        let checkInValue = data.checkInTime
            ? new Date(data.checkInTime).toISOString().slice(0, 16)
            : '';
        let checkOutValue = data.checkOutTime
            ? new Date(data.checkOutTime).toISOString().slice(0, 16)
            : '';

        let html = '';

        if (!editMode) {
            // Modalità visualizzazione
            html = `
                <p><strong>Nome:</strong> ${data.nome}</p>
                <p><strong>Cognome:</strong> ${data.cognome}</p>
                <p><strong>Ditta:</strong> ${data.ditta ?? '-'}</p>
                <p><strong>Referente:</strong> ${data.referente ?? '-'}</p>
                <p><strong>Data visita:</strong> ${new Date(data.dataVisita).toLocaleDateString()}</p>
                <p><strong>Check-in:</strong> ${data.checkInTime
                    ? new Date(data.checkInTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
                    : '—'
                }</p>
                <p><strong>Check-out:</strong> ${data.checkOutTime
                    ? new Date(data.checkOutTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
                    : 'Non ancora effettuato'
                }</p>
                <hr />
                <p><strong>QR Code:</strong></p>
                ${qrHtml}
                <hr />
                <button type="button" id="editVisitorBtn" class="btn btn-warning">
                    ✏️ Modifica
                </button>
            `;
        } else {
            // Modalità modifica
            html = `
                <div class="mb-3">
                    <label class="form-label"><strong>Nome:</strong></label>
                    <input type="text" id="editNome" class="form-control" value="${data.nome}">
                </div>
                <div class="mb-3">
                    <label class="form-label"><strong>Cognome:</strong></label>
                    <input type="text" id="editCognome" class="form-control" value="${data.cognome}">
                </div>
                <div class="mb-3">
                    <label class="form-label"><strong>Ditta:</strong></label>
                    <input type="text" id="editDitta" class="form-control" value="${data.ditta ?? ''}">
                </div>
                <div class="mb-3">
                    <label class="form-label"><strong>Referente:</strong></label>
                    <input type="text" id="editReferente" class="form-control" value="${data.referente ?? ''}">
                </div>
                <div class="mb-3">
                    <label class="form-label"><strong>Data visita:</strong></label>
                    <input type="date" id="editDataVisita" class="form-control" value="${new Date(data.dataVisita).toISOString().split('T')[0]}">
                </div>
                <div class="mb-3">
                    <label class="form-label"><strong>Check-in:</strong></label>
                    <input type="datetime-local" id="editCheckIn" class="form-control" value="${checkInValue}">
                </div>
                <div class="mb-3">
                    <label class="form-label"><strong>Check-out:</strong></label>
                    <input type="datetime-local" id="editCheckOut" class="form-control" value="${checkOutValue}">
                </div>
                <hr />
                <p><strong>QR Code:</strong> <em class="text-muted">(non modificabile)</em></p>
                ${qrHtml}
                <hr />
                <div class="d-flex gap-2">
                    <button type="button" id="saveVisitorBtn" class="btn btn-success">
                        💾 Salva
                    </button>
                    <button type="button" id="cancelEditBtn" class="btn btn-secondary">
                        ❌ Annulla
                    </button>
                </div>
            `;
        }

        $('#detailsModal .modal-body').html(html);
    }

    // Passa alla modalità modifica
    $(document).on('click', '#editVisitorBtn', function () {
        isEditMode = true;
        $.getJSON('/Presence/DetailsJson', { presenceId: currentVisitorId }, function (data) {
            renderModalContent(data, true);
        });
    });

    // Annulla modifica
    $(document).on('click', '#cancelEditBtn', function () {
        isEditMode = false;
        $.getJSON('/Presence/DetailsJson', { presenceId: currentVisitorId }, function (data) {
            renderModalContent(data, false);
        });
    });

    // Salva modifiche
    $(document).on('click', '#saveVisitorBtn', function () {
        const editedVisitor = {
            Id: currentVisitorId,
            Nome: $('#editNome').val(),
            Cognome: $('#editCognome').val(),
            Ditta: $('#editDitta').val(),
            Referente: $('#editReferente').val(),
            DataVisita: $('#editDataVisita').val(),
            CheckInTime: $('#editCheckIn').val() || null,
            CheckOutTime: $('#editCheckOut').val() || null
        };

        $.ajax({
            url: '/Visitor/EditVisitor',
            type: 'POST',
            data: editedVisitor,
            success: function (response) {
                alert('Visitatore aggiornato con successo!');

                // Ricarica la pagina per aggiornare tutto
                location.reload();
            },
            error: function (xhr) {
                alert('Errore durante l\'aggiornamento: ' + xhr.responseText);
            }
        });
    });
});

// Force checkout functionality
$(document).on('click', '.force-checkout-btn', function () {
    if (!confirm("Confermi il check-out del visitatore?"))
        return;

    const button = $(this);
    const presenceId = button.data('presence-id');

    $.ajax({
        url: '/Presence/ForceCheckOut',
        type: 'POST',
        data: {
            presenceId: presenceId,
            __RequestVerificationToken:
                $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                // Disabilita bottone
                button.prop('disabled', true);

                // Aggiorna stato
                const row = button.closest('tr');
                row.find('.visit-status')
                    .html('<span class="badge bg-secondary">Visita terminata</span>');

                // Aggiorna check-out time
                row.find('.checkout-time')
                    .text(response.checkOutTime);
            }
        },
        error: function () {
            alert("Errore durante il check-out");
        }
    });
});
