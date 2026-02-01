const { createApp } = Vue;

window.visitorListApp = createApp({
    data() {
        return {
            visitors: window.initialVisitors.map(v => ({
                ...v,
                _visible: true
            })),
            searchText: '',
            selectedDate: ''
        };
    },

    computed: {
        filteredVisitors() {
            return this.visitors.filter(v => {

                // 🔍 filtro testo
                const textMatch =
                    !this.searchText ||
                    v.nome.toLowerCase().includes(this.searchText.toLowerCase()) ||
                    v.cognome.toLowerCase().includes(this.searchText.toLowerCase());

                // 📅 filtro data
                let dateMatch = true;
                if (this.selectedDate && v.checkIn) {
                    // checkIn: dd/MM/yyyy HH:mm
                    const datePart = v.checkIn.split(' ')[0];
                    const [dd, mm, yyyy] = datePart.split('/');
                    const formatted = `${yyyy}-${mm}-${dd}`;
                    dateMatch = formatted === this.selectedDate;
                }

                return textMatch && dateMatch;
            });
        }
    },

    methods: {

        // ➕ aggiunta nuovo visitatore
        addVisitor(visitor) {
            this.visitors.unshift({
                ...visitor,
                _visible: true
            });
        },

        // 🗑️ elimina visitatore
        removeVisitor(visitorId) {
            this.visitors = this.visitors.filter(v => v.id !== visitorId);
        },

        // 🔍 dettagli visita
        openDetails(visitor) {
            if (!visitor.currentPresenceId) return;

            // reset modal
            visitorModalApp.loading = true;
            visitorModalApp.visitor = null;

            $.getJSON('/Presence/DetailsJson', {
                presenceId: visitor.currentPresenceId
            })
                .done(data => {
                    visitorModalApp.visitor = data;
                    visitorModalApp.loading = false;
                })
                .fail(() => {
                    visitorModalApp.loading = false;
                    alert('Errore nel caricamento dei dettagli');
                });
        },

        // 🧹 reset filtri
        resetFilters() {
            this.searchText = '';
            this.selectedDate = '';
        }
    }
}).mount('#visitorListApp');
