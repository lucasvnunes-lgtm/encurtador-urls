using Xunit;
using Moq;
using EncurtadorURLs.Services;
using EncurtadorURLs.Data;
using EncurtadorURLs.Models;

namespace EncurtadorURLs.Tests.Services;

/// <summary>
/// Testes unitários para UrlsService.CreateShortUrlAsync.
/// FASE RED do TDD: estes testes foram escritos ANTES da implementação existir.
/// Todos devem FALHAR neste momento — isso é o esperado e desejado.
/// </summary>
public class UrlsServiceTests
{
    private readonly Mock<IUrlContext> _mockContext;
    private readonly UrlsService _service;

    public UrlsServiceTests()
    {
        _mockContext = new Mock<IUrlContext>();
        _service = new UrlsService(_mockContext.Object);
    }

    [Fact]
    public async Task CreateShortUrlAsync_ComUrlValida_DeveRetornarUrlItemComShortCode()
    {
        // Arrange
        var urlOriginal = "https://www.google.com";
        _mockContext.Setup(c => c.ShortCodeExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var resultado = await _service.CreateShortUrlAsync(urlOriginal);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(urlOriginal, resultado.OriginalUrl);
        Assert.False(string.IsNullOrEmpty(resultado.ShortCode));
        _mockContext.Verify(c => c.AddAsync(It.IsAny<UrlItem>()), Times.Once);
    }

    [Fact]
    public async Task CreateShortUrlAsync_ComUrlVazia_DeveLancarArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateShortUrlAsync(string.Empty)
        );
    }

    [Fact]
    public async Task CreateShortUrlAsync_ComEsquemaInvalido_DeveLancarArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateShortUrlAsync("ftp://arquivo-invalido.com")
        );
    }

    [Fact]
    public async Task CreateShortUrlAsync_ComUrlNula_DeveLancarArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateShortUrlAsync(null)
        );
    }

    [Fact]
    public async Task CreateShortUrlAsync_ComUrlMaiorLimite_DeveLancarArgumentException()
    {
        string url = new string('a', 2100);
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateShortUrlAsync("https://exemplo.com/" + new string('a', 2100))
        );
    }


    [Fact]
    public async Task GetUrlByIdAsync_ComIdInexiste_DeveRetornarNulo()
    {
        var guidInexistente = Guid.NewGuid();
        _mockContext.Setup(c => c.GetByIdAsync(guidInexistente)).ReturnsAsync((UrlItem)null);
        var resultado = await _service.GetUrlByIdAsync(guidInexistente);
        Assert.Null(resultado);
    }


    [Fact]
    public async Task GetUrlByIdAsync_ComIdVazia_DeveLancarArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetUrlByIdAsync(Guid.Empty)
        );
    }

    [Fact]
    public async Task GetUrlByIdAsync_ComIdExiste_DeveRetornarUrlItem()
    {
        var guidExistente = Guid.NewGuid();
        UrlItem urlItem = new UrlItem
        {
            Id = guidExistente,
            OriginalUrl = "http://nada.com.br",
            ShortCode = "abc123"
        };

        _mockContext.Setup(c => c.GetByIdAsync(guidExistente)).ReturnsAsync(urlItem);

        var resultado = await _service.GetUrlByIdAsync(guidExistente);

        Assert.NotNull(resultado);
        Assert.Equal(urlItem.OriginalUrl, resultado.OriginalUrl);
        Assert.Equal(urlItem.Id, resultado.Id);
        Assert.Equal(urlItem.ShortCode, resultado.ShortCode);
    }
}