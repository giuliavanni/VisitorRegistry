// Force checkout functionality
$(document).on('click', '.force-checkout-btn', function () {

    const btn = $(this);
    const visitorId = btn.data('visitor-id');

    console.log("CLICK", visitorId);

    $.ajax({
        url: '/Visitor/UpdatePresence',
        type: 'POST',
        data: {
            visitorId: visitorId,
            mode: 'out'
        },
        success: function (res) {
            console.log("SUCCESS", res);

            if (!res.success) {
                alert("Checkout non riuscito");
                return;
            }

            const row = btn.closest('tr');

            // aggiorna la cella checkout
            row.find('.checkout-time').text(res.checkoutTime);

            // aggiorna badge stato → "Visita terminata" grigio
            const badge = row.find('.status-badge');
            badge.text("Visita terminata");
            badge.removeClass('bg-success bg-warning');
            badge.addClass('bg-secondary');

            // rimuovi il pulsante
            btn.remove();

        },
        error: function (xhr) {
            console.log("ERROR", xhr.status, xhr.responseText);
            alert("Errore durante check out");
        }
    });
});


