using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet_CBD
{
    public static class Session
    {
        // Stocke l'ID unique de l'utilisateur (utile pour les réservations)
        public static int UtilisateurId { get; set; }

        // Stocke l'email pour l'affichage
        public static string Email { get; set; }

        // Stocke le rôle (admin_principal, admin_secondaire, membre)
        public static string Role { get; set; }

        // Méthode pour vider la session lors de la déconnexion
        public static void Deconnexion()
        {
            UtilisateurId = 0;
            Email = null;
            Role = null;
        }
    }
}