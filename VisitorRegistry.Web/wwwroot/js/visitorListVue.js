const { createApp } = Vue;

createApp({
    data() {
        return {
            visitors: window.initialVisitors,

            // ===== GENERALE =====
            loading: false,
            search: '',
            showOnlyInProgress: false,
            showOnlyToday: false,
            filterDate: null,

            // ===== VISITE IN CORSO =====
            visitor: null,
            editVisitor: null,
            editMode: false,
            modal: null,

            // ===== VISITE PROGRAMMATE =====
            plannedVisitor: null,
            plannedEditMode: false,
            plannedModal: null,
            plannedEditVisitor: null,

            // ===== NUOVO VISITATORE =====
            addingVisitor: false,
            newVisitor: {
                Nome: '',
                Cognome: '',
                CheckIn: '',
                CheckOut: '',
                DataVisita: ''
            }
        };
    },

    computed: {
        filteredVisitors() {
            return this.visitors.filter(v => {
                const fullName = `${v.Nome} ${v.Cognome}`.toLowerCase();
                const matchName =
                    !this.search || fullName.includes(this.search.toLowerCase());

                const matchInProgress =
                    !this.showOnlyInProgress ||
                    (v.CheckIn && !v.CheckOut);

                let matchToday = true;
                if (this.showOnlyToday) {
                    if (!v.CheckIn) return false;
                    matchToday =
                        new Date(v.CheckIn).toDateString() ===
                        new Date().toDateString();
                }

                return matchName && matchInProgress && matchToday;
            });
        }
    },

    mounted() {
        this.modal = new bootstrap.Modal(
            document.getElementById('detailsModal')
        );

        this.plannedModal = new bootstrap.Modal(
            document.getElementById('plannedDetailsModal')
        );
    },

    methods: {
        // ==========================
        // VISITE IN CORSO
        // ==========================
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
            this.editVisitor = { ...this.visitor };
            this.editMode = true;
        },

        cancelEdit() {
            this.editMode = false;
            this.editVisitor = null;
        },

        // ==========================
        // VISITE PROGRAMMATE
        // ==========================
        openPlannedDetails(visitorId) {
            this.loading = true;
            this.plannedVisitor = null;
            this.plannedEditMode = false;

            fetch(`/Visitor/DetailsPlannedJson?visitorId=${visitorId}`)
                .then(r => r.json())
                .then(data => {

                    if (data.dataVisita) {
                        data.dataVisita = this.toDateTimeLocal(data.dataVisita);
                    }

                    this.plannedVisitor = data;
                    this.plannedModal.show();
                })
                .finally(() => {
                    this.loading = false;
                });
        },

        enablePlannedEdit() {
            this.plannedEditMode = true;
        },


        cancelPlannedEdit() {
            this.plannedEditMode = false;
        },

        savePlannedEdit() {
            const payload = {
                Id: this.plannedVisitor.visitorId,
                Nome: this.plannedVisitor.nome,
                Cognome: this.plannedVisitor.cognome,
                DataVisita: this.plannedVisitor.dataVisita,
                Ditta: this.plannedVisitor.ditta,
                Referente: this.plannedVisitor.referente
            };

            fetch('/Visitor/EditPlanned', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: new URLSearchParams(payload)
            })
                .then(r => {
                    if (!r.ok) throw new Error("Errore salvataggio");
                    return r.json();
                })
                .then(data => {
                    this.plannedVisitor = data; 
                    this.plannedEditMode = false;
                })
                .catch(() => {
                    alert("Errore salvataggio visita programmata");
                });
        },

        startAddVisitor() {
            this.addingVisitor = true;
            this.newVisitor = {
                Nome: '',
                Cognome: '',
                CheckIn: '',
                CheckOut: '',
                DataVisita: ''
            };
        },

        cancelAddVisitor() {
            this.addingVisitor = false;
        },

        cancelNewVisitor() {
            this.addingVisitor = false;
        },

        saveNewVisitor() {
            fetch('/Visitor/AddVisitor', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: new URLSearchParams(this.newVisitor)
            })
                .then(r => {
                    if (!r.ok) throw new Error('Errore salvataggio');
                    return r.json();
                })
                .then(data => {
                    this.visitors.push({
                        Id: data.id,
                        Nome: data.nome,
                        Cognome: data.cognome,
                        CheckIn: this.newVisitor.CheckIn,
                        CheckOut: this.newVisitor.CheckOut,
                        DataVisita: this.newVisitor.DataVisita,
                        CurrentPresenceId: data.currentPresenceId
                    });

                    this.addingVisitor = false;
                })
                .catch(err => {
                    console.error(err);
                    alert('Errore durante il salvataggio del visitatore');
                });
        },


        deleteVisitor(visitorId) {
            if (!confirm('Sei sicura di voler eliminare il visitatore?')) return;

            fetch(`/Visitor/Delete?id=${visitorId}`, {
                method: 'POST'
            })
                .then(r => r.json())
                .then(res => {
                    if (res.success) {
                        this.visitors = this.visitors.filter(v => v.Id !== visitorId);
                    } else {
                        alert('Errore durante l’eliminazione');
                    }
                });
        },

        forceCheckout(visitor) {
            if (!confirm(
                `Forzare il check-out per ${visitor.Nome} ${visitor.Cognome}?`
            )) return;

            this.loading = true;

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
                .then(r => r.json())
                .then(res => {
                    if (!res.success) {
                        alert(res.message || 'Errore check-out');
                        return;
                    }

                    // aggiorna la riga in tabella
                    visitor.CheckOut = res.checkOutTime;
                })
                .finally(() => {
                    this.loading = false;
                });
        },

        // ==========================
        // UTILITY
        // ==========================
        formatDateTime(value) {
            if (!value) return '—';
            const d = new Date(value);
            return isNaN(d)
                ? '—'
                : d.toLocaleString('it-IT');
        },

        toDateTimeLocal(value) {
            if (!value) return null;

            const d = new Date(value);
            if (isNaN(d)) return null;

            const pad = n => n.toString().padStart(2, '0');

            return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
        },

        printTable() {
            window.print();
        }
    }
}).mount('#visitorApp');
