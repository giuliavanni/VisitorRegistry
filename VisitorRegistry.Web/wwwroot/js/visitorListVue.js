const { createApp } = Vue;

createApp({
    data() {
        return {
            visitors: window.initialVisitors,
            visitor: null,
            editVisitor: null,
            loading: false,
            editMode: false,
            search: '',
            modal: null,

            showOnlyInProgress: false,
            filterDate: null,
        };
    },

    computed: {
        filteredVisitors() {
            return this.visitors.filter(v => {

                /* filtro nome */
                const fullName =
                    `${v.Nome} ${v.Cognome}`.toLowerCase();

                const matchName =
                    !this.search ||
                    fullName.includes(this.search.toLowerCase());

                /* filtro data */
                let matchDate = true;
                if (this.filterDate) {
                    if (!v.CheckIn) return false;

                    const checkInDate =
                        new Date(v.CheckIn).toISOString().split('T')[0];

                    matchDate = checkInDate === this.filterDate;
                }

                /* solo visite in corso */
                const matchInProgress =
                    !this.showOnlyInProgress ||
                    (v.CheckIn && !v.CheckOut);

                return matchName && matchDate && matchInProgress;
            });
        }
    },

    mounted() {
        this.modalEl = document.getElementById('detailsModal');
        this.modal = new bootstrap.Modal(this.modalEl);

        this.modalEl.addEventListener('shown.bs.modal', () => {
            this.renderQr();
        });
    },

    methods: {
        openDetails(presenceId) {
            this.loading = true;
            this.visitor = null;
            this.editMode = false;

            this.modal.show();

            fetch(`/Presence/DetailsJson?presenceId=${presenceId}`)
                .then(r => r.json())
                .then(data => {
                    this.visitor = data;
                })
                .finally(() => {
                    this.loading = false;
                });
        },
        enableEdit() {
            this.editVisitor = JSON.parse(JSON.stringify(this.visitor));
            this.editMode = true;
        },

        cancelEdit() {
            this.editMode = false;
            this.editVisitor = null;
        },

        save() {
            const payload = {
                Id: this.editVisitor.visitorId,
                PresenceId: this.editVisitor.presenceId,
                Nome: this.editVisitor.nome,
                Cognome: this.editVisitor.cognome,
                Ditta: this.editVisitor.ditta,
                Referente: this.editVisitor.referente
            };

            this.loading = true;

            fetch('/Visitor/EditVisitor', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: new URLSearchParams(payload)
            })
                .then(r => {
                    if (!r.ok) throw new Error();
                    return r.json();
                })

                .then(() => {
                    return fetch(`/Presence/DetailsJson?presenceId=${payload.PresenceId}`);
                })
                .then(r => r.json())
                .then(data => {

                    // aggiorna modal
                    this.visitor = {
                        ...this.visitor,
                        nome: data.nome,
                        cognome: data.cognome,
                        ditta: data.ditta,
                        referente: data.referente
                    };

                    // aggiorna tabella
                    const index = this.visitors.findIndex(
                        v => v.CurrentPresenceId === data.presenceId
                    );

                    if (index !== -1) {
                        this.visitors[index] = {
                            ...this.visitors[index],
                            Nome: data.nome,
                            Cognome: data.cognome,
                            CheckIn: data.checkInTime,
                            CheckOut: data.checkOutTime
                        };
                    }

                    this.editMode = false;
                    this.editVisitor = null;
                })
                .catch(() => {
                    alert('Errore durante il salvataggio');
                })
                .finally(() => {
                    this.loading = false;
                });
        },

        formatDateTime(value) {
            if (!value) return '—';

            const d = new Date(value);

            if (isNaN(d)) return '—';

            return d.toLocaleString('it-IT', {
                day: '2-digit',
                month: '2-digit',
                year: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
        },
        forceCheckout(visitor) {
            if (!confirm('Forzare il checkout del visitatore?')) return;

            fetch('/Visitor/UpdatePresence', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: new URLSearchParams({
                    visitorId: visitor.Id,
                    mode: 'out'
                })
            })
                .then(r => {
                    if (!r.ok) throw new Error();
                    return r.json();
                })
                .then(res => {
                    if (!res.success) {
                        alert('Checkout non riuscito');
                        return;
                    }

                    // aggiorna tabella
                    visitor.CheckOut = res.checkoutTime;

                    // aggiorna modal se aperto
                    if (this.visitor &&
                        this.visitor.presenceId === visitor.CurrentPresenceId) {
                        this.visitor.checkOutTime = res.checkoutTime;
                    }
                })
                .catch(() => {
                    alert('Errore durante il force checkout');
                });
        },
        printTable() {
            window.print();
        },
        renderQr() {
            if (!this.visitor || !this.visitor.qrCode) return;

            const el = document.getElementById('qrCode');
            if (!el) return;

            el.innerHTML = '';

            new QRCode(el, {
                text: this.visitor.qrCode,
                width: 180,
                height: 180
            });
        },  

    }
}).mount('#visitorApp');
