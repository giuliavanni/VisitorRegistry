$(document).ready(function () {
    $('#detailsModal').on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget);
        var presenceId = button.data('presence-id');
        var modal = $(this);

        // Reset modal content mentre carica
        modal.find('.modal-body').html('Caricamento...');
        $.getJSON('/Presence/DetailsJson', { presenceId: presenceId }, function (data) {

            let qrHtml = data.qrCodeImageBase64
                ? `<img src="data:image/png;base64,${data.qrCodeImageBase64}" style="max-width:200px;" />
           <br />
           <code>${data.qrCode}</code>`
                : `<em>QR code non disponibile</em>`;

            var html = `
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
    `;

            $('#detailsModal .modal-body').html(html);
        });

    });
});
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


