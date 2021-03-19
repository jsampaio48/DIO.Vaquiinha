using FluentAssertions;
using System.Threading.Tasks;
using Vaquinha.Domain.Extensions;
using Vaquinha.Integration.Tests.Fixtures;
using Vaquinha.MVC;
using Xunit;

namespace Vaquinha.Integration.Tests
{
    [Collection(nameof(IntegrationWebTestsFixtureCollection))]
    public class HomeTests
    {
        private readonly IntegrationTestsFixture<StartupWebTests> _integrationTestsFixture;

        public HomeTests(IntegrationTestsFixture<StartupWebTests> integrationTestsFixture)
        {
            _integrationTestsFixture = integrationTestsFixture;
        }

        [Trait("HomeControllerIntegrationTests", "HomeController_CarregarPaginaInicial_TotalDoadoresETotalValorArrecadadoDeveSerZero")]
        [Fact]
        public async Task HomeController_CarregarPaginaInicial_TotalDoadoresETotalValorArrecadadoDeveSerZero()
        {
            // Arrange & Act
            var home = await _integrationTestsFixture.Client.GetAsync("Home");

            // Assert
            home.EnsureSuccessStatusCode();
            var dadosHome = await home.Content.ReadAsStringAsync();

            var totalArrecadado = 0.ToDinheiroBrString();
            var metaCampanha = _integrationTestsFixture.ConfiguracaoGeralAplicacao.MetaCampanha.ToDinheiroBrString();

            // Dados totais da doação
            dadosHome.Should().Contain(expected: "Arrecadamos quanto?");
            dadosHome.Should().Contain(expected: totalArrecadado);

            dadosHome.Should().Contain(expected: "Quanto falta arrecadar?");
            dadosHome.Should().Contain(expected: metaCampanha);
        }

        [Trait("DoadoresControllerIntegrationTests", "DoadoresController_CarregarPagina_TotalDoadores")]
        [Fact]
        public async Task DoadoresController_CarregarPagina_TotalDoadores()
        {
            // Arrange & Act
            var home = await _integrationTestsFixture.Client.GetAsync("Doadores");

            // Assert
            home.EnsureSuccessStatusCode();
            var dadosHome = await home.Content.ReadAsStringAsync();

            // Dados totais da doação
            dadosHome.Should().Contain(expected: "Veja quem já doou!");
            dadosHome.Should().Contain(expected: "Ainda não houveram doações. Seja você o primeiro!");
        }

        [Trait("DoacoesControllerIntegrationTests", "DoacoesController_CarregarPagina")]
        [Fact]
        public async Task DoacoesController_CarregarPagina()
        {
            // Arrange & Act
            var home = await _integrationTestsFixture.Client.GetAsync("Doacoes/Create");

            // Assert
            home.EnsureSuccessStatusCode();
            var dadosHome = await home.Content.ReadAsStringAsync();

            // Dados totais da doação
            dadosHome.Should().Contain(expected: "Doe agora");
        }

    }



}