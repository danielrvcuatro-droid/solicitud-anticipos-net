using SolicitudAnticipos.Domain.Entities;
using SolicitudAnticipos.Domain.Excepciones;
using SolicitudAnticipos.Domain.Politicas;
using Xunit;

namespace SolicitudAnticipos.Domain.Tests;

public class ConstructorPlanAprobacionTests
{
    [Fact]
    public void Excluye_al_analista_financiero_cuando_no_se_requiere()
    {
        var matriz = new List<MatrizAprobacion>
        {
            MatrizAprobacion.Crear("Desarrollos", nivel: 1, orden: 1, "gerente@rvcuatro.com", "Gerente"),
            MatrizAprobacion.Crear("Desarrollos", nivel: 2, orden: 1, "analista@rvcuatro.com", "Analista Financiero", esAnalistaFinanciero: true),
        };

        var plan = ConstructorPlanAprobacion.Construir(matriz, incluyeAnalistaFinanciero: false);

        Assert.Single(plan);
        Assert.Equal("gerente@rvcuatro.com", plan[0].AprobadorEmail);
    }

    [Fact]
    public void Incluye_al_analista_financiero_cuando_se_requiere()
    {
        var matriz = new List<MatrizAprobacion>
        {
            MatrizAprobacion.Crear("Desarrollos", nivel: 1, orden: 1, "gerente@rvcuatro.com", "Gerente"),
            MatrizAprobacion.Crear("Desarrollos", nivel: 2, orden: 1, "analista@rvcuatro.com", "Analista Financiero", esAnalistaFinanciero: true),
        };

        var plan = ConstructorPlanAprobacion.Construir(matriz, incluyeAnalistaFinanciero: true);

        Assert.Equal(2, plan.Count);
        Assert.Equal("analista@rvcuatro.com", plan[1].AprobadorEmail);
        Assert.Equal(2, plan[1].Nivel);
    }

    [Fact]
    public void Ignora_filas_inactivas()
    {
        var inactiva = MatrizAprobacion.Crear("Corporativo", nivel: 1, orden: 1, "exempleado@rvcuatro.com", "Ex Empleado");
        inactiva.Desactivar();

        var matriz = new List<MatrizAprobacion>
        {
            inactiva,
            MatrizAprobacion.Crear("Corporativo", nivel: 1, orden: 2, "director@rvcuatro.com", "Director"),
        };

        var plan = ConstructorPlanAprobacion.Construir(matriz, incluyeAnalistaFinanciero: false);

        Assert.Single(plan);
        Assert.Equal("director@rvcuatro.com", plan[0].AprobadorEmail);
    }

    [Fact]
    public void Renumera_el_orden_de_forma_consecutiva_tras_filtrar()
    {
        var matriz = new List<MatrizAprobacion>
        {
            MatrizAprobacion.Crear("Leasing", nivel: 1, orden: 5, "gerente@rvcuatro.com", "Gerente"),
            MatrizAprobacion.Crear("Leasing", nivel: 2, orden: 9, "director@rvcuatro.com", "Director"),
        };

        var plan = ConstructorPlanAprobacion.Construir(matriz, incluyeAnalistaFinanciero: false);

        Assert.Equal(1, plan[0].Orden);
        Assert.Equal(2, plan[1].Orden);
    }

    [Fact]
    public void Lanza_excepcion_si_no_quedan_aprobadores_activos()
    {
        var matriz = new List<MatrizAprobacion>();

        Assert.Throws<SolicitudDomainException>(() =>
            ConstructorPlanAprobacion.Construir(matriz, incluyeAnalistaFinanciero: false));
    }
}
