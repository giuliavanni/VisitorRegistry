$(document).ready(function () {

    // Ricerca per nome
    $('#searchName').on('keyup', function () {
        var value = $(this).val().toLowerCase();
        $("#visitorTable tbody tr").not('#newVisitorRowTable').filter(function () {
            $(this).toggle($(this).find('td:eq(1)').text().toLowerCase().indexOf(value) > -1);
        });
    });

    // Filtra per data check-in
    $('#filterDate').on('change', function () {
        var selectedDate = $(this).val(); // yyyy-MM-dd

        $("#visitorTable tbody tr").not('#newVisitorRowTable').each(function () {
            var checkInText = $(this).find('td:eq(3)').text().trim();

            if (checkInText === '—' || selectedDate === '') {
                $(this).show();
                return;
            }

            // checkInText: dd/MM/yyyy HH:mm
            var datePart = checkInText.split(' ')[0]; // dd/MM/yyyy
            var parts = datePart.split('/');

            // cambiamento a yyyy-MM-dd
            var formattedDate = parts[2] + '-' + parts[1] + '-' + parts[0];

            $(this).toggle(formattedDate === selectedDate);
        });
    });


    // Mostra/Nasconde riga aggiunta nuovo visitatore
    $('#addVisitorBtn').click(function () {
        $('#newVisitorRowTable').toggle();
    });

    // Stampa tabella
    $('#printBtn').click(function () {
        window.print();
    });

    // Aggiungi nuovo visitatore 
    $('#saveNewVisitorBtn').click(function (e) {
        e.preventDefault();

        var newVisitor = {
            Nome: $('#newNome').val(),
            Cognome: $('#newCognome').val(),
            Ditta: $('#newDitta').val(),
            Referente: $('#newReferente').val(),
            DataVisita: $('#newCheckIn').val(),
            QrCode: '',
            CheckIn: $('#newCheckIn').val(),
            CheckOut: $('#newCheckOut').val()
        };

        $.ajax({
            url: '/Visitor/AddVisitor',
            type: 'POST',
            data: newVisitor,
            success: function (response) {
                // --- badge stato visita ---
                let statoHtml = '';
                if (response.statoVisita === "Visita programmata") {
                    statoHtml = `<span class="badge bg-warning status-badge">Visita programmata</span>`;
                }
                else if (response.statoVisita === "Visita in corso") {
                    statoHtml = `<span class="badge bg-success status-badge">Visita in corso</span>`;
                }
                else if (response.statoVisita === "Visita terminata") {
                    statoHtml = `<span class="badge bg-secondary status-badge">Visita terminata</span>`;
                }

                // --- colonna checkout ---
                let checkoutHtml = '';
                if (response.checkOut && response.checkOut !== "—") {
                    checkoutHtml = response.checkOut;
                }
                else if (response.checkIn && response.checkIn !== "—") {
                    checkoutHtml = `
            <button class="btn btn-outline-warning btn-sm force-checkout-btn"
                    data-visitor-id="${response.id}"
                    data-presence-id="${response.currentPresenceId}">
                Forza Check-Out
            </button>`;
                }

                
                let dettagliBtn = '';
                if (response.currentPresenceId) {
                    dettagliBtn = `
                <button class="btn btn-info btn-sm"
                        data-bs-toggle="modal"
                        data-bs-target="#detailsModal"
                        data-presence-id="${response.currentPresenceId}">
                    Dettagli visita
                </button>`;
                }

                var newRow = `<tr>
                 <td>${response.nome}</td>
                 <td>${response.cognome}</td>

                 <td class="visit-status">
               ${statoHtml}
               </td>

              <td>${response.checkIn ?? ""}</td>

              <td class="checkout-time">
              ${checkoutHtml}
              </td>

        <td class="d-flex gap-1">
            ${dettagliBtn}

            <button class="btn btn-danger btn-sm delete-visitor-btn"
                    data-visitor-id="${response.id}">
                🗑️ Elimina
            </button>
        </td>
    </tr>`;

                $('#visitorTable tbody').prepend(newRow);

                // reset campi e nasconde riga
                $('#newVisitorRowTable input').val('');
                $('#newVisitorRowTable').hide();
            },
            error: function (xhr) {
                alert('Errore durante l\'aggiunta del visitatore: ' + xhr.responseText);
            }
        });
    });
    $(document).on('click', '.delete-visitor-btn', function () {

        if (!confirm("⚠️ Sei sicuro di voler eliminare questo visitatore"))
            return;

        const visitorId = $(this).data('visitor-id');

        $.ajax({
            url: '/Visitor/CancelVisitor',
            type: 'POST',
            data: {
                id: visitorId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function () {
                alert('✅ Il visitatore è stato eliminato con successo');
                location.reload();
            },
            error: function () {
                alert('❌ Si è verificato un errore durante la cancellazione ');
            }
        });
    });


    // Cancella aggiunta nuovo visitatore
    $('#cancelNewVisitorBtn').click(function () {
        $('#newVisitorRowTable input').val('');
        $('#newVisitorRowTable').hide();
    });
});
