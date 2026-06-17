using System.ComponentModel.DataAnnotations;

namespace SIMBG.Compartido.Modelos
{
    public class GestorDocumento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public string TipoDoc { get; set; } = string.Empty;

        [Required]
        public string RutaArchivo { get; set; } = string.Empty;

        [Required]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Aprobado, Rechazado

        //debe ser opcional porque al subir el documento, el gestor no puede mandar un comentario, sólo el estado
        public string? Comentario { get; set; }
    }
}
