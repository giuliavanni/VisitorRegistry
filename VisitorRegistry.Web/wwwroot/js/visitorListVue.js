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
            showOnlyToday: false,

            addingVisitor: false,
            newVisitor: {
                Nome: '',
                Cognome: '',
                CheckIn: '',
                CheckOut: ''
            }
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

                /* solo visite di oggi */
                let matchToday = true;
                if (this.showOnlyToday) {
                    if (!v.CheckIn) return false;

                    const today = new Date();
                    today.setHours(0, 0, 0, 0);

                    const checkIn = new Date(v.CheckIn);
                    checkIn.setHours(0, 0, 0, 0);

                    matchToday = checkIn.getTime() === today.getTime();
                }

                return matchName && matchDate && matchInProgress && matchToday;
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
            if (!presenceId) {
                console.warn("presenceId è null — impossibile caricare i dettagli");
                return;
            }
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
                    console.log('FORCE CHECKOUT RESPONSE:', res);

                    if (!res.success) {
                        alert(res.message);
                        return;
                    }

                    const checkoutTime = res.checkOutTime;

                    const index = this.visitors.findIndex(v => v.Id === visitor.Id);
                    if (index !== -1) {
                        this.visitors[index] = {
                            ...this.visitors[index],
                            CheckOut: checkoutTime,
                            StatoVisita: 'Uscito'
                        };
                    }
                    if (
                        this.visitor &&
                        this.visitor.presenceId === visitor.CurrentPresenceId
                    ) {
                        this.visitor = {
                            ...this.visitor,
                            checkOutTime: checkoutTime,
                            presenceId: null
                        };
                    }
                    return;
                })
                .catch(err => {
                    console.error(err);
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
        startAddVisitor() {
            this.addingVisitor = true;
            this.newVisitor = {
                Nome: '',
                Cognome: '',
                CheckIn: '',
                CheckOut: ''
            };
        },

        cancelNewVisitor() {
            this.addingVisitor = false;
            this.newVisitor = {};
        },

        saveNewVisitor() {
            if (!this.newVisitor.Nome || !this.newVisitor.Cognome) {
                alert('Nome e Cognome obbligatori');
                return;
            }

            // Costruisci il body solo con i campi che hanno valore
            const params = new URLSearchParams();
            params.append('Nome', this.newVisitor.Nome);
            params.append('Cognome', this.newVisitor.Cognome);

            // Invia CheckIn solo se è stato compilato
            if (this.newVisitor.CheckIn) {
                params.append('CheckIn', this.newVisitor.CheckIn);
            }
            // Invia CheckOut solo se è stato compilato
            if (this.newVisitor.CheckOut) {
                params.append('CheckOut', this.newVisitor.CheckOut);
            }

            fetch('/Visitor/AddVisitor', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: params
            })
                .then(r => {
                    if (!r.ok) throw new Error();
                    return r.json();
                })
                .then(data => {
                    // Usa i valori che ha restituito il backend, non quelli locali
                    this.visitors.unshift({
                        Id: data.id,
                        Nome: data.nome,
                        Cognome: data.cognome,
                        CheckIn: this.newVisitor.CheckIn || null,
                        CheckOut: this.newVisitor.CheckOut || null,
                        CurrentPresenceId: data.currentPresenceId
                    });

                    this.addingVisitor = false;
                })
                .catch(() => {
                    alert('Errore durante il salvataggio');
                });
        
        },
        deleteVisitor(visitorId) {
            if (!confirm("Sei sicuro di voler eliminare questo visitatore?")) return;

            // disabilita interazioni durante la richiesta
            this.loading = true;

            fetch('/Visitor/Delete', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: new URLSearchParams({ id: visitorId })
            })
                .then(r => r.json())
                .then(res => {
                    if (res.success) {
                        this.visitors = this.visitors.filter(v => v.Id !== visitorId);
                    } else {
                        alert("Errore durante l'eliminazione");
                    }
                })
                .catch(() => {
                    alert("Errore durante l'eliminazione");
                })
                .finally(() => {
                    this.loading = false;
                });
        }

    }
}).mount('#visitorApp');
