

Grille = {
    indicateurErreur: false,

    grilles: function () {
        return ($("[data-role='grid']")) ? $("[data-role='grid']") : [];
    },

    chargerDonnees: function (grilleKendo) {

        if (grilleKendo) {
            Grille.Etat.chargerOptions();
            grilleKendo.dataSource.read();
        }
    },

    requestStart: function () {
        var grilles = Grille.grilles();
        if (grilles.length === 1) {
            var grilleId = grilles[0].id;

            console.time("requete");
            //Une correction a été faite fait pour éviter que la barre de progression ne soit en double 
            //dans le cas du retrait d'un filtre
            kendo.ui.progress($("#" + grilleId), true);
        }
    },

    requestEnd: function (e) {

        var grilles = Grille.grilles();

        if (grilles.length === 1) {

            var grilleId = Grille.grilles()[0].id;
            kendo.ui.progress($("#" + grilleId), false);


            console.timeEnd("requete");            
            console.time("rafraichissement");
            setTimeout(Grille.attendreFinTraitement, 100);
        }
    },



    attendreFinTraitement: function () {


        if ($("[data-role='grid']").find(".k-loading-mask").length < 1) {
            var grilleId = Grille.grilles()[0].id;
            console.timeEnd("rafraichissement");
            console.log("nombre d'enregistrement: " + $("[data-uid]").length);
        } else {
            setTimeout(Grille.attendreFinRafraichissement(), 100);
        }
    },


    commonDatabound: function (e) {

        //On doit mettre un délais parce que c'est Kendo
        lancerFonctionApresDelais(initialiserGridExport);
        this.initialiserSelecteurDateGrille();
        this.Etat.selectionneLignes(e);
    }   
};