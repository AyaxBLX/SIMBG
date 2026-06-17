using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace SIMBG.Web.Auth

{
    public class ProveedorAutenticacionJWT : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        public ProveedorAutenticacionJWT(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }
        // Este método lo llama Blazor automáticamente para saber si el usuario está logueado
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("tokenJwt");

            // Si no hay token, eres un usuario anónimo (sin sesión)
            if (string.IsNullOrWhiteSpace(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Si hay token, lo desciframos para ver quién es
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }

        // Llamaremos a este método desde la pantalla de Login cuando entres con éxito
        public void NotificarUsuarioLogueado(string token)
        {
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        // Llamaremos a este método desde el botón de "Cerrar Sesión"
        public void NotificarUsuarioDeslogueado()
        {
            var anonimo = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonimo)));
        }

        // --- Funciones auxiliares para descifrar el JSON Web Token ---
        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs != null)
            {
                foreach (var kvp in keyValuePairs)
                {
                    // EL TRUCO ESTÁ AQUÍ: Traducimos las etiquetas comprimidas del JSON a las oficiales de C#
                    string tipoClaim = kvp.Key;

                    if (kvp.Key == "role" || kvp.Key == "roles")
                    {
                        tipoClaim = ClaimTypes.Role;
                    }
                    else if (kvp.Key == "unique_name" || kvp.Key == "name")
                    {
                        tipoClaim = ClaimTypes.Name;
                    }

                    claims.Add(new Claim(tipoClaim, kvp.Value.ToString() ?? ""));
                }
            }
            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}