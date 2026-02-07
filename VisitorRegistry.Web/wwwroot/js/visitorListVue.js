const { createApp } = Vue;

createApp({
    data() {
        return {
            visitors: window.initialVisitors || [],

            // ===== GENERALE =====
            loading: false,
            search: '',
            toastMessage: '',

            // ===== VISITA IN CORSO =====
            visitor: null,
            editVisitor: null,
            editMode: false,
            detailsModal: null,
            currentVisitorId: null,

            // ===== VISITA PROGRAMMATA =====
            plannedVisitor: null,
            plannedEditMode: false,
            plannedModal: null,

            // ===== NUOVO VISITATORE =====
            addModal: null,
            newVisitor: {
                Nome: '',
                Cognome: '',
                Ditta: '',
                Referente: '',
                CheckIn: '',
                CheckOut: '',
                DataVisita: ''
            },

            // ===== TOAST =====
            successToast: null,
            errorToast: null,

            // ===== CONFERMA ELIMINAZIONE =====
            deleteConfirmModal: null,
            pendingDeleteAction: null
        };
    },

    computed: {
        filteredVisitors() {
            if (!this.search) return this.visitors;

            const s = this.search.toLowerCase();
            return this.visitors.filter(v =>
                `${v.Nome} ${v.Cognome}`.toLowerCase().includes(s)
            );
        }
    },

    mounted() {
        this.detailsModal = new bootstrap.Modal(
            document.getElementById('detailsModal')
        );
        this.plannedModal = new bootstrap.Modal(
            document.getElementById('plannedDetailsModal')
        );
        this.addModal = new bootstrap.Modal(
            document.getElementById('addVisitorModal')
        );
        this.deleteConfirmModal = new bootstrap.Modal(
            document.getElementById('deleteConfirmModal')
        );

        this.successToast = new bootstrap.Toast(
            document.getElementById('successToast')
        );
        this.errorToast = new bootstrap.Toast(
            document.getElementById('errorToast')
        );
    },

    methods: {
        /* =========================
           TOAST
        ========================= */
        showSuccess(msg) {
            this.toastMessage = msg;
            this.successToast.show();
        },

        showError(msg) {
            this.toastMessage = msg;
            this.errorToast.show();
        },

        /* =========================
           CONFERMA ELIMINAZIONE
        ========================= */
        confirmDelete(action) {
            this.pendingDeleteAction = action;
            this.deleteConfirmModal.show();
        },

        executeDelete() {
            if (this.pendingDeleteAction) {
                this.pendingDeleteAction();
                this.pendingDeleteAction = null;
            }
            this.deleteConfirmModal.hide();
        },

        cancelDelete() {
            this.pendingDeleteAction = null;
            this.deleteConfirmModal.hide();
        },

        /* =========================
           FORZA CHECK-OUT
        ========================= */
        forceCheckout(v) {
            this.loading = true;

            fetch(`/Visitor/UpdatePresence?visitorId=${v.Id}&mode=out`, {
                method: 'POST'
            })
                .then(r => r.json())
                .then(res => {
                    if (!res.success) throw new Error();

                    v.CheckOut = res.checkOutTime;
                    this.showSuccess('✅ Check-out effettuato con successo');
                })
                .catch(() => {
                    this.showError('❌ Errore durante il check-out');
                })
                .finally(() => {
                    this.loading = false;
                });
        },

        /* =========================
           DOWNLOAD QR CODE
        ========================= */
        downloadQRCode(type) {
            let qrCode, fileName, containerId;

            if (type === 'visitor') {
                qrCode = this.visitor.qrCode;
                fileName = `${this.visitor.nome}_${this.visitor.cognome}`;
                containerId = 'qrCodeDisplay';
            } else {
                qrCode = this.plannedVisitor.qrCode;
                fileName = `${this.plannedVisitor.nome}_${this.plannedVisitor.cognome}`;
                containerId = 'qrCodeDisplayPlanned';
            }

            // Ottieni il canvas del QR code esistente
            const qrCanvas = document.querySelector(`#${containerId} canvas`);

            if (qrCanvas) {
                // Usa il canvas esistente
                const link = document.createElement('a');
                link.download = `QRCode_${fileName}.png`;
                link.href = qrCanvas.toDataURL('image/png');
                link.click();
                this.showSuccess('📥 QR Code scaricato con successo');
            } else {
                // Crea un QR code temporaneo se non esiste
                const tempDiv = document.createElement('div');
                const qr = new QRCode(tempDiv, {
                    text: qrCode,
                    width: 256,
                    height: 256
                });

                setTimeout(() => {
                    const canvas = tempDiv.querySelector('canvas');
                    if (canvas) {
                        const link = document.createElement('a');
                        link.download = `QRCode_${fileName}.png`;
                        link.href = canvas.toDataURL('image/png');
                        link.click();
                        tempDiv.remove();
                        this.showSuccess('📥 QR Code scaricato con successo');
                    } else {
                        this.showError('❌ Errore durante la generazione del QR Code');
                    }
                }, 100);
            }
        },

        /* =========================
           AGGIUNGI VISITATORE
        ========================= */
        openAddModal() {
            this.newVisitor = {
                Nome: '',
                Cognome: '',
                Ditta: '',
                Referente: '',
                CheckIn: '',
                CheckOut: '',
                DataVisita: ''
            };
            this.addModal.show();
        },

        saveNewVisitor() {
            if (!this.newVisitor.Nome || !this.newVisitor.Cognome) {
                this.showError('❌ Nome e Cognome sono obbligatori');
                return;
            }

            fetch('/Visitor/AddVisitor', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: new URLSearchParams(this.newVisitor)
            })
                .then(r => r.json())
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

                    this.addModal.hide();
                    this.showSuccess('✅ Visitatore aggiunto con successo');
                })
                .catch(() => {
                    this.showError('❌ Errore durante il salvataggio del visitatore');
                });
        },

        /* =========================
           DETTAGLI VISITA
        ========================= */
        openDetails(presenceId, visitorId) {
            this.loading = true;
            this.visitor = null;
            this.editMode = false;
            this.currentVisitorId = visitorId;

            this.detailsModal.show();

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

        save() {
            const payload = {
                Id: this.editVisitor.visitorId,
                PresenceId: this.editVisitor.presenceId,
                Nome: this.editVisitor.nome,
                Cognome: this.editVisitor.cognome,
                Ditta: this.editVisitor.ditta,
                Referente: this.editVisitor.referente
            };

            fetch('/Visitor/EditVisitor', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: new URLSearchParams(payload)
            })
                .then(r => r.json())
                .then(data => {
                    this.visitor.nome = data.nome;
                    this.visitor.cognome = data.cognome;
                    this.visitor.ditta = data.ditta;
                    this.visitor.referente = data.referente;

                    const i = this.visitors.findIndex(v => v.Id === data.visitorId);
                    if (i !== -1) {
                        this.visitors[i].Nome = data.nome;
                        this.visitors[i].Cognome = data.cognome;
                    }

                    this.editMode = false;
                    this.editVisitor = null;
                    this.showSuccess('✅ Modifiche salvate con successo');
                })
                .catch(() => {
                    this.showError('❌ Errore durante il salvataggio');
                });
        },

        deleteVisitorFromModal() {
            this.confirmDelete(() => {
                this.loading = true;

                fetch(`/Visitor/Delete?id=${this.currentVisitorId}`, { method: 'POST' })
                    .then(r => r.json())
                    .then(res => {
                        if (!res.success) throw new Error();

                        this.visitors = this.visitors.filter(
                            v => v.Id !== this.currentVisitorId
                        );
                        this.detailsModal.hide();
                        this.showSuccess('✅ Visitatore eliminato con successo');
                    })
                    .catch(() => {
                        this.showError('❌ Errore durante l\'eliminazione');
                    })
                    .finally(() => {
                        this.loading = false;
                    });
            });
        },

        /* =========================
           VISITE PROGRAMMATE
        ========================= */
        openPlannedDetails(visitorId) {
            this.loading = true;
            this.plannedVisitor = null;
            this.plannedEditMode = false;

            fetch(`/Visitor/DetailsPlannedJson?visitorId=${visitorId}`)
                .then(r => r.json())
                .then(data => {
                    if (data.dataVisita)
                        data.dataVisita = this.toDateTimeLocal(data.dataVisita);

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
                .then(r => r.json())
                .then(data => {
                    this.plannedVisitor.nome = data.nome;
                    this.plannedVisitor.cognome = data.cognome;
                    this.plannedVisitor.ditta = data.ditta;
                    this.plannedVisitor.referente = data.referente;
                    this.plannedVisitor.dataVisita = data.dataVisita;

                    const i = this.visitors.findIndex(v => v.Id === data.visitorId);
                    if (i !== -1) {
                        this.visitors[i].Nome = data.nome;
                        this.visitors[i].Cognome = data.cognome;
                    }

                    this.plannedEditMode = false;
                    this.showSuccess('✅ Modifiche salvate con successo');
                })
                .catch(() => {
                    this.showError('❌ Errore durante il salvataggio');
                });
        },

        deleteVisitorFromPlannedModal() {
            this.confirmDelete(() => {
                this.loading = true;

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

        /* =========================
           UTILS
        ========================= */
        formatDateTime(v) {
            if (!v) return '—';
            const d = new Date(v);
            return isNaN(d) ? '—' : d.toLocaleString('it-IT');
        },

        toDateTimeLocal(v) {
            const d = new Date(v);
            if (isNaN(d)) return null;
            const p = n => n.toString().padStart(2, '0');
            return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}T${p(d.getHours())}:${p(d.getMinutes())}`;
        }
    }
}).mount('#visitorApp');
