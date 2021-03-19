using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using Vaquinha.Tests.Common.Fixtures;
using Xunit;

namespace Vaquinha.AutomatedUITests
{
	public class DoacaoTests : IDisposable, IClassFixture<DoacaoFixture>, 
                                               IClassFixture<EnderecoFixture>, 
                                               IClassFixture<CartaoCreditoFixture>
	{
		private DriverFactory _driverFactory = new DriverFactory();
		private IWebDriver _driver;

		private readonly DoacaoFixture _doacaoFixture;
		private readonly EnderecoFixture _enderecoFixture;
		private readonly CartaoCreditoFixture _cartaoCreditoFixture;

		public DoacaoTests(DoacaoFixture doacaoFixture, EnderecoFixture enderecoFixture, CartaoCreditoFixture cartaoCreditoFixture)
        {
            _doacaoFixture = doacaoFixture;
            _enderecoFixture = enderecoFixture;
            _cartaoCreditoFixture = cartaoCreditoFixture;
        }
		public void Dispose()
		{
			_driverFactory.Close();
		}

		[Fact]
		public void DoacaoUI_AcessoTelaHome()
		{
			// Arrange
			_driverFactory.NavigateToUrl("https://vaquinha.azurewebsites.net/");
			_driver = _driverFactory.GetWebDriver();

			// Act
			IWebElement webElement = null;
			webElement = _driver.FindElement(By.ClassName("vaquinha-logo"));

			// Assert
			webElement.Displayed.Should().BeTrue(because:"logo exibido");
		}
		[Fact]
		public void DoacaoUI_CriacaoDoacao()
		{
			//Arrange
			var doacao = _doacaoFixture.DoacaoValida();
            doacao.AdicionarEnderecoCobranca(_enderecoFixture.EnderecoValido());
            doacao.AdicionarFormaPagamento(_cartaoCreditoFixture.CartaoCreditoValido());
			_driverFactory.NavigateToUrl("https://vaquinha.azurewebsites.net/");
			_driver = _driverFactory.GetWebDriver();

			//Act
			IWebElement webElement = null;
			webElement = _driver.FindElement(By.ClassName("btn-yellow"));
			webElement.Click();

			//Assert
			_driver.Url.Should().Contain("/Doacoes/Create");
		}

		[Fact]
		[Trait("Doacao", "Doacao_CorretamentePreenchidos_DoacaoValida")]
		public void Doacao_CorretamentePreenchidos_DoacaoValida()
        {
			//Arrange
			var doacao = _doacaoFixture.DoacaoValida();
			doacao.AdicionarEnderecoCobranca(_enderecoFixture.EnderecoValido());
			doacao.AdicionarFormaPagamento(_cartaoCreditoFixture.CartaoCreditoValido());

			//Act
			var valido = doacao.Valido();

			//Assert
			valido.Should().BeTrue(because: "Os campos foram preenchidos corrretamente");
			doacao.ErrorMessages.Should().BeEmpty();
        }
		
		[Fact]
		[Trait("Doacao", "Doacao_DadosPessoaisInvalidos_DoacaoInvalida")]
		public void Doacao_DadosPessoaisInvalidos_DoacaoInvalida()
		 {
			//Arrange
			const bool EMAIL_INVALIDO = true;
			var doacao = _doacaoFixture.DoacaoValida(EMAIL_INVALIDO);
			doacao.AdicionarEnderecoCobranca(_enderecoFixture.EnderecoValido());
			doacao.AdicionarFormaPagamento(_cartaoCreditoFixture.CartaoCreditoValido());

			//Act
			var valido = doacao.Valido();

			//Assert
			valido.Should().BeFalse(because: "O campo e-mail est� inv�lido");
			doacao.ErrorMessages.Should().Contain("O campo Email � inv�lido.");
			doacao.ErrorMessages.Should().HaveCount(1, because: "somente o campo email est� inv�lido");
		}

		[Theory]
		[InlineData(0)]
		[InlineData(-10)]
		[InlineData(4)]
		[InlineData(-10.20)]
		[InlineData(-55.4)]
		[InlineData(-0.1)]
		[Trait("Doacao", "Doacao_ValoresDoacaoMenorIgualZero_DoacaoInvalida")]
		public void Doacao_ValoresDoacaoMenorIgualZero_DoacaoInvalida(double valorDoacao)
		{
			//Arrange
			var doacao = _doacaoFixture.DoacaoValida(false,valorDoacao);
			doacao.AdicionarEnderecoCobranca(_enderecoFixture.EnderecoValido());
			doacao.AdicionarFormaPagamento(_cartaoCreditoFixture.CartaoCreditoValido());

			//Act
			var valido = doacao.Valido();

			//Assert
			valido.Should().BeFalse(because: "O campo valor est� inv�lido");
			doacao.ErrorMessages.Should().Contain("Valor m�nimo de doa��o � de R$ 5,00");
			doacao.ErrorMessages.Should().HaveCount(1, because: "somente o campo valor est� inv�lido");
		}

		[Theory]
		[InlineData(4501)]
		[InlineData(5001)]
		[InlineData(4505)]
		[InlineData(4500.01)]
		[Trait("Doacao", "Doacao_ValoresDoacaoMaiorLimite_DoacaoInvalida")]
		public void Doacao_ValoresDoacaoMaiorLimite_DoacaoInvalida(double valorDoacao)
		{
			//Arrange
			const bool EXCEDER_MAX_VALOR_DOACAO = true;
			var doacao = _doacaoFixture.DoacaoValida(false, valorDoacao);
			doacao.AdicionarEnderecoCobranca(_enderecoFixture.EnderecoValido());
			doacao.AdicionarFormaPagamento(_cartaoCreditoFixture.CartaoCreditoValido());

			//Act
			var valido = doacao.Valido();

			//Assert
			valido.Should().BeFalse(because: "O campo valor est� inv�lido");
			doacao.ErrorMessages.Should().Contain("Valor m�ximo para a doa��o � de R$4.500,00");
			doacao.ErrorMessages.Should().HaveCount(1, because: "somente o campo valor est� inv�lido");
		}

		[Fact]
		[Trait("Doacao", "Doacao_DadosNaoInformados_DoacaoInvalida")]
		public void Doacao_DadosNaoInformados_DoacaoInvalida()
		{
			//Arrange
			var doacao = _doacaoFixture.DoacaoInvalida(false);
			doacao.AdicionarEnderecoCobranca(_enderecoFixture.EnderecoValido());
			doacao.AdicionarFormaPagamento(_cartaoCreditoFixture.CartaoCreditoValido());

			//Act
			var valido = doacao.Valido();

			//Assert
			valido.Should().BeFalse(because: "Os campos de doa��o n�o foram informados");
			doacao.ErrorMessages.Should().HaveCount(3, because: "Os 3 campos obrigat�rios da doa��o n�o foram preenchidos");
			doacao.ErrorMessages.Should().Contain("O campo Nome � obrigat�rio.", because: "o campo Nome n�o foi informado");
			doacao.ErrorMessages.Should().Contain("O campo Email � obrigat�rio.", because: "o campo Email n�o foi informado");
			doacao.ErrorMessages.Should().Contain("Valor m�nimo de doa��o � de R$ 5,00", because: "valor m�nimo de doa��o n�o foi atingido");
		}

		[Fact]
		[Trait("Doacao", "Doacao_DadosNaoInformadosDoacaoAnonima_DoacaoInvalida")]
		public void Doacao_DadosNaoInformadosDoacaoAnonima_DoacaoInvalida()
		{
			//Arrange
			var doacao = _doacaoFixture.DoacaoInvalida(true);
			doacao.AdicionarEnderecoCobranca(_enderecoFixture.EnderecoValido());
			doacao.AdicionarFormaPagamento(_cartaoCreditoFixture.CartaoCreditoValido());

			//Act
			var valido = doacao.Valido();

			//Assert
			valido.Should().BeFalse(because: "Os campos de doa��o n�o foram informados");
			doacao.ErrorMessages.Should().HaveCount(2, because: "Os 2 campos obrigat�rios da doa��o n�o foram preenchidos");
			doacao.ErrorMessages.Should().Contain("Valor m�nimo de doa��o � de R$ 5,00", because: "valor m�nimo de doa��o n�o foi atingido");
			doacao.ErrorMessages.Should().Contain("O campo Email � obrigat�rio.", because: "o campo Email n�o foi informado");
		}


	}
}