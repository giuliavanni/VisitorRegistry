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
            var checkInText = $(this).find('td:eq(4)').text().trim();

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
                var newRow = `<tr>
                    <td>${response.id}</td>
                    <td>${response.nome}</td>
                    <td>${response.cognome}</td>
                    <td>${response.statoVisita}</td>
                    <td>${response.checkIn}</td>
                    <td>${response.checkOut}</td>
                    <td>
                        <button class="btn btn-info btn-sm me-1" data-bs-toggle="modal" data-bs-target="#detailsModal" data-presence-id="${response.currentPresenceId}">Dettagli visita</button>
                        <a class="btn btn-warning btn-sm" href="/Visitor/Edit/${response.id}">Modifica</a>
                    </td>
                </tr>`;

                $('#visitorTable tbody').prepend(newRow);

                // Resetta campi e nasconde riga
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
