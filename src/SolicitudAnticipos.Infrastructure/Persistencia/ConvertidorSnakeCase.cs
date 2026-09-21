using System.Text;

namespace SolicitudAnticipos.Infrastructure.Persistencia;

/// <summary>
/// Convierte identificadores PascalCase/camelCase (como los genera EF Core por convención)
/// a snake_case, para que las tablas y columnas en Supabase/Postgres se vean como
/// "solicitudes", "aprobador_email", etc. en vez de "Solicitudes", "AprobadorEmail".
/// </summary>
internal static class ConvertidorSnakeCase
{
    public static string Convertir(string identificador)
    {
        if (string.IsNullOrEmpty(identificador))
        {
            return identificador;
        }

        var resultado = new StringBuilder(identificador.Length + 10);

        for (var i = 0; i < identificador.Length; i++)
        {
            var caracterActual = identificador[i];

            if (char.IsUpper(caracterActual))
            {
                var esInicio = i == 0;
                var precedidoPorMinuscula = i > 0 && char.IsLower(identificador[i - 1]);
                var seguidoPorMinuscula = i + 1 < identificador.Length && char.IsLower(identificador[i + 1]);

                if (!esInicio && (precedidoPorMinuscula || seguidoPorMinuscula))
                {
                    resultado.Append('_');
                }

                resultado.Append(char.ToLowerInvariant(caracterActual));
            }
            else
            {
                resultado.Append(caracterActual);
            }
        }

        return resultado.ToString();
    }
}
