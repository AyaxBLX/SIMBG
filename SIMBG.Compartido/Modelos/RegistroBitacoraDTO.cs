using System;
using System.Collections.Generic;
using System.Text;

namespace SIMBG.Compartido.Modelos
{
    public class RegistroBitacoraDTO
    {
        public string Matricula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public DateTime FechaEntrada { get; set; }
        public DateTime? FechaSalida { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}