using System.Data;

namespace Projet_CBD
{
    internal class Program
    {
        // Instance unique pour gérer la base de données
        static DatabaseManager db = new DatabaseManager();

        static void Main(string[] args)
        {
            Console.Title = "Gestion Salle de Sport - ESILV";
            Console.WriteLine("=== BIENVENUE À LA SALLE DE SPORT ===");

            // 1. CONNEXION
            Console.Write("Email : ");
            string email = Console.ReadLine();
            Console.Write("Mot de passe : ");
            string mdp = Console.ReadLine();

            // On vérifie les identifiants et on récupère le rôle
            string role = db.VerifierConnexion(email, mdp);

            if (role != null)
            {
                // On remplit la session pour l'utiliser partout dans l'appli
                Session.Email = email;
                Session.Role = role;

                // Note : Dans un cas réel, il faudrait aussi récupérer l'UtilisateurId via la DB
                // Ici on simule un ID pour l'exemple
                Session.UtilisateurId = 1;

                Console.WriteLine($"\nConnexion réussie ! Bienvenue {role}");

                // Direction le bon menu selon le rôle
                if (role.ToLower().Contains("admin"))
                    MenuAdmin();
                else
                    MenuMembre();
            }
            else
            {
                Console.WriteLine("\nIdentifiants incorrects ou compte inactif. Fin du programme.");
            }

            Console.WriteLine("\nAppuyez sur une touche pour quitter...");
            Console.ReadKey();
        }

        // ==========================================
        // --- INTERFACE GÉRANT (ADMIN) ---
        // ==========================================
        static void MenuAdmin()
        {
            while (true)
            {
                Console.WriteLine("\n--- MENU ADMINISTRATION ---");
                Console.WriteLine("1. Afficher tous les membres (Jointure)");
                Console.WriteLine("2. Ajouter un membre");
                Console.WriteLine("3. Afficher les coachs");
                Console.WriteLine("4. Créer un nouveau cours");
                Console.WriteLine("5. INTERFACE ÉVALUATION (Statistiques)");

                // Condition de privilège pour l'admin principal
                if (Session.Role == "admin_principal")
                {
                    Console.WriteLine("6. Supprimer un membre (Privilège Principal)");
                }

                Console.WriteLine("0. Déconnexion");
                Console.Write("Choix : ");
                string choix = Console.ReadLine();

                if (choix == "0") break;

                switch (choix)
                {
                    case "1":
                        // Requête avec JOINTURE (exigée par le sujet)
                        DataTable dtM = db.ExecuteQuery("SELECT m.nom, m.prenom, u.email FROM membre m JOIN Utilisateur u ON m.id_utilisateur = u.id_utilisateur");
                        Console.WriteLine("\nListe des membres :");
                        foreach (DataRow r in dtM.Rows) Console.WriteLine($"- {r["nom"]} {r["prenom"]} ({r["email"]})");
                        break;

                    case "2": // Ajouter un membre
                        Console.Write("Nom : "); string n = Console.ReadLine();
                        Console.Write("Prénom : "); string p = Console.ReadLine();
                        Console.Write("Email : "); string e = Console.ReadLine();
                        Console.Write("Mot de passe : "); string m = Console.ReadLine();

                        // 1. Créer l'utilisateur d'abord (id_role 3 = membre)
                        db.ExecuteNonQuery($"INSERT INTO Utilisateur (email, mot_de_passe, id_role, actif) VALUES ('{e}', '{m}', 3, 1)");

                        // 2. Récupérer l'ID qui vient d'être créé
                        DataTable resId = db.ExecuteQuery("SELECT LAST_INSERT_ID()");
                        int newId = Convert.ToInt32(resId.Rows[0][0]);

                        // 3. Créer le membre lié à cet utilisateur
                        db.ExecuteNonQuery($"INSERT INTO membre (nom, prenom, date_inscription, id_utilisateur) VALUES ('{n}', '{p}', NOW(), {newId})");

                        Console.WriteLine("Membre et compte utilisateur créés ! Il apparaîtra maintenant dans la liste.");

                        break;
                    case "3": // Afficher les coachs
                        Console.WriteLine("\n--- LISTE DES COACHS ---");
                        // On utilise un LEFT JOIN pour voir les coachs même sans compte utilisateur
                        string sqlCoachs = "SELECT nom, prenom, specialite FROM coach";
                        DataTable dtC = db.ExecuteQuery(sqlCoachs);

                        if (dtC != null && dtC.Rows.Count > 0)
                        {
                            foreach (DataRow r in dtC.Rows)
                                Console.WriteLine($"- {r["prenom"]} {r["nom"]} ({r["specialite"]})");
                        }
                        else { Console.WriteLine("Aucun coach trouvé."); }
                        break;
                    case "4": // Créer un nouveau cours
                        Console.WriteLine("\n--- CRÉATION D'UN NOUVEAU COURS ---");
                        Console.Write("Nom du cours : "); string nomC = Console.ReadLine();
                        Console.Write("Durée (minutes) : "); int duree = int.Parse(Console.ReadLine());
                        Console.Write("Capacité Max : "); int cap = int.Parse(Console.ReadLine());

                        // Afficher les coachs pour aider l'admin à choisir un ID
                        DataTable dtCoachs = db.ExecuteQuery("SELECT id_coach, nom FROM coach");
                        Console.WriteLine("Coachs disponibles (ID) :");
                        foreach (DataRow r in dtCoachs.Rows) Console.WriteLine($"ID {r["id_coach"]} : {r["nom"]}");

                        Console.Write("ID du coach responsable : "); int idCoach = int.Parse(Console.ReadLine());

                        // Requête d'insertion
                        string insertCours = $"INSERT INTO Cours (nom_cours, duree_minutes, horaire, capacite_max, statut_cours, id_coach) " +
                                             $"VALUES ('{nomC}', {duree}, NOW(), {cap}, 'Actif', {idCoach})";
                        db.ExecuteNonQuery(insertCours);
                        Console.WriteLine("Cours créé avec succès !");
                        break;

                    case "5":
                        AfficherStats();
                        break;

                    case "6":
                        if (Session.Role == "admin_principal")
                        {
                            Console.Write("ID du membre à supprimer : ");
                            int id = int.Parse(Console.ReadLine());
                            db.ExecuteNonQuery($"DELETE FROM membre WHERE id_membre = {id}");
                            Console.WriteLine("Membre supprimé.");
                        }
                        break;
                }
            }
        }

        // ==========================================
        // --- INTERFACE ÉVALUATION (STATS) ---
        // ==========================================
        static void AfficherStats()
        {
            Console.WriteLine("\n=== STATISTIQUES GLOBALES (Fonctions d'agrégation) ===");

            // 1. COUNT
            var count = db.ExecuteQuery("SELECT COUNT(*) FROM membre");
            Console.WriteLine($"- Total des membres : {count.Rows[0][0]}");

            // 2. AVG
            var avg = db.ExecuteQuery("SELECT AVG(capacite_max) FROM cours");
            Console.WriteLine($"- Capacité moyenne des cours : {avg.Rows[0][0]} places");

            // 3. MAX
            var max = db.ExecuteQuery("SELECT MAX(capacite_max) FROM cours");
            Console.WriteLine($"- Plus grand cours : {max.Rows[0][0]} places");

            // 4. MIN
            var min = db.ExecuteQuery("SELECT MIN(capacite_max) FROM cours");
            Console.WriteLine($"- Plus petit cours : {min.Rows[0][0]} places");

            // 5. SUM
            var sum = db.ExecuteQuery("SELECT SUM(capacite_max) FROM cours");
            Console.WriteLine($"- Capacité totale cumulée : {sum.Rows[0][0]} places");

            // 6. Requête complexe (Coach le plus suivi)
            string sqlCoach = "SELECT c.nom, COUNT(r.id_reservation) as total FROM coach c JOIN cours co ON c.id_coach = co.id_coach JOIN reservation r ON co.id_cours = r.id_cours GROUP BY c.nom ORDER BY total DESC LIMIT 1";
            var topCoach = db.ExecuteQuery(sqlCoach);
            if (topCoach.Rows.Count > 0)
                Console.WriteLine($"- Coach star : {topCoach.Rows[0]["nom"]} ({topCoach.Rows[0]["total"]} réservations)");
        }

        // ==========================================
        // --- INTERFACE MEMBRE ---
        // ==========================================
        static void MenuMembre()
        {
            while (true)
            {
                Console.WriteLine("\n--- MENU MEMBRE ---");
                Console.WriteLine("1. S'inscrire à un cours (Vérif Capacité)");
                Console.WriteLine("2. Voir mon historique");
                Console.WriteLine("3. Annuler une réservation");
                Console.WriteLine("0. Déconnexion");
                Console.Write("Choix : ");

                string choix = Console.ReadLine();
                if (choix == "0") break;

                if (choix == "1")
                {
                    Console.Write("ID du cours : ");
                    int idC = int.Parse(Console.ReadLine());
                    // Appel de la méthode qui contient la SOUS-REQUÊTE (exigée)
                    bool ok = db.InscrireMembre(Session.UtilisateurId, idC);
                    Console.WriteLine(ok ? "Inscription confirmée !" : "Erreur : Cours complet ou inexistant.");
                }

                if (choix == "2")
                {
                    // Historique avec Jointure
                    DataTable hist = db.ExecuteQuery($"SELECT c.nom_cours FROM reservation r JOIN cours c ON r.id_cours = c.id_cours WHERE r.id_membre = {Session.UtilisateurId}");
                    foreach (DataRow r in hist.Rows) Console.WriteLine($"- {r["nom_cours"]}");
                }
                if (choix == "3")
                {
                    Console.WriteLine("\n--- ANNULATION DE RÉSERVATION ---");
                    // 1. Afficher les réservations actuelles pour que l'utilisateur voie quoi annuler
                    string sqlListe = $@"SELECT r.id_reservation, c.nom_cours 
                         FROM reservation r 
                         JOIN cours c ON r.id_cours = c.id_cours 
                         WHERE r.id_membre = {Session.UtilisateurId}";

                    DataTable dt = db.ExecuteQuery(sqlListe);

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow r in dt.Rows)
                            Console.WriteLine($"ID Réservation : {r["id_reservation"]} - Cours : {r["nom_cours"]}");

                        Console.Write("\nEntrez l'ID de la réservation à annuler : ");
                        int idResa = int.Parse(Console.ReadLine());

                        // 2. Exécuter la suppression SQL
                        db.ExecuteNonQuery($"DELETE FROM reservation WHERE id_reservation = {idResa} AND id_membre = {Session.UtilisateurId}");
                        Console.WriteLine("Réservation annulée avec succès !");
                    }
                    else
                    {
                        Console.WriteLine("Vous n'avez aucune réservation active.");
                    }
                }
            }
        }
    }
}
