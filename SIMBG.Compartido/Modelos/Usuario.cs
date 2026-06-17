using System.ComponentModel.DataAnnotations;

namespace SIMBG.Compartido.Modelos
{
    public class Usuario
    {
        [Key] //se refiere a la Primary Key
        public int Id { get; set; }

        [Required] //Signfica que es obligatorio y que no puede ser not null
        [StringLength(20)]
        public string Matricula { get; set; } = string.Empty;

        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string? ApellidoMaterno { get; set; } //aunque puede ser nulo

        [Required]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        //Si no mandamos ningún rol al crear el usuario, quedará por defecto Estudiante
        [Required] 
        public string Rol { get; set; } = "Estudiante";

        public bool Activo { get; set; } = true;

    }
}
