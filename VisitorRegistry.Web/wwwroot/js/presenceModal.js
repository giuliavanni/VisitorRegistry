$(document).ready(function () {
    $('#detailsModal').on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget);
        var presenceId = button.data('presence-id');
        var modal = $(this);

        // Reset modal content mentre carica
        modal.find('.modal-body').html('Caricamento...');

        $.getJSON('/Presence/Details/' + presenceId, function (data) {
            var html = `
                <p><strong>Nome:</strong> ${data.nome}</p>
                <p><strong>Cognome:</strong> ${data.cognome}</p>
                <p><strong>Ditta:</strong> ${data.ditta}</p>
                <p><strong>Referente:</strong> ${data.referente}</p>
                <p><strong>Data visita:</strong> ${new Date(data.dataVisita).toLocaleDateString()}</p>
                <p><strong>Check-in:</strong> ${data.checkInTime ? new Date(data.checkInTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : '—'}</p>
                <p><strong>Check-out:</strong> ${data.checkOutTime ? new Date(data.checkOutTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : 'Non ancora effettuato'}</p>
            `;
            modal.find('.modal-body').html(html);
        });
    });
});

