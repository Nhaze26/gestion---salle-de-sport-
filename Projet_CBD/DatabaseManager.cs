using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using MySql.Data.MySqlClient; 

namespace Projet_CBD
{
    internal class DatabaseManager
    {
        // Remplacez par vos identifiants créés dans utilisateurs.sql
        private string connectionString = "Server=localhost;Database=salle_sport;Uid=admin_principal;Pwd=motdepasse;";

        // Méthode pour lire des données (ex: afficher les membres ou les cours)
        public DataTable ExecuteQuery(string query)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erreur : " + ex.Message);
                    return null;
                }
            }
        }

        // Méthode pour modifier des données (ex: ajouter un membre, valider une inscription) 
        public void ExecuteNonQuery(string query)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erreur : " + ex.Message);
                }
            }
        }
        public string VerifierConnexion(string email, string password)
        {
            // On cherche le rôle de l'utilisateur avec son email et MDP
            string query = @"SELECT r.nom_role FROM Utilisateur u 
                 JOIN Role r ON u.id_role = r.id_role 
                 WHERE u.email='" + email + "' AND u.mot_de_passe='" + password + "' AND u.actif=TRUE";
            DataTable res = ExecuteQuery(query);

            if (res != null && res.Rows.Count > 0)
            {
                // Si on trouve une ligne, on renvoie le rôle (admin_principal, membre, etc.)
                return res.Rows[0]["nom_role"].ToString();
            }
            return null; // Sinon, on renvoie null (identifiants faux)
        }

        // 4. MÉTHODE D'INSCRIPTION (Optionnelle mais recommandée ici aussi)
        public bool InscrireMembre(int idMembre, int idCours)
        {
            string sqlCapacite = $@"
                SELECT (capacite_max - (SELECT COUNT(*) FROM Reservation WHERE id_cours = {idCours})) 
                FROM Cours WHERE id_cours = {idCours}";

            DataTable dt = ExecuteQuery(sqlCapacite);
            if (dt == null || dt.Rows.Count == 0) return false;

            int placesLibres = Convert.ToInt32(dt.Rows[0][0]);

            if (placesLibres > 0)
            {
                ExecuteNonQuery($"INSERT INTO Reservation (id_membre, id_cours, date_reservation, statut_reservation) VALUES ({idMembre}, {idCours}, NOW(), 'Confirmé')");
                return true;
            }
            return false;
        }
    }
}
