using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BatalhaNaval.Application.DTOs;
using FluentAssertions;
using FluentAssertions.Execution;

namespace BatalhaNaval.IntegrationTests;

[Collection("Sequential")]
public class UsersTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private const string EndpointUsers = "/users";
    private const string EndpointLogin = "/auth/login";
    private const string EndpointProfile = "/users/profile";
    private const string EndpointRanking = "/users/player_stats";
    private readonly HttpClient _client;

    public UsersTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Deve_Executar_Jornada_Completa_Do_Usuario()
    {
        var usuario = new { Username = "ComandanteTeste", Password = "SenhaForte123!" };

        // STEP 0: Verificar rotas protegidas
        await Passo_VerificarRotasProtegidas();

        // STEP 1: Registro
        await Passo_RegistrarUsuarioComSucesso(usuario);

        // STEP 2: Validação de Duplicidade
        await Passo_TentarRegistrarUsuarioDuplicado(usuario);

        // STEP 3: Segurança (Login Inválido)
        await Passo_BloquearLoginComSenhaErrada(usuario.Username);

        // STEP 4: Autenticação (Login Sucesso) e Obtenção do Token
        var token = await Passo_RealizarLoginComSucesso(usuario);

        // STEP 5: Rota Protegida (Perfil)
        await Passo_ValidarPerfilInicialDoJogador(token);
    }

    #region Steps

    private async Task Passo_VerificarRotasProtegidas()
    {
        var responseProfile = await _client.GetAsync(EndpointProfile);
        responseProfile.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "Usuário não autenticado deve receber 401 Unauthorized ao acessar Profile Info");

        var responseRanking = await _client.GetAsync(EndpointRanking);
        responseRanking.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "Usuário não autenticado deve receber 401 Unauthorized ao acessar Ranking");
    }

    private async Task Passo_RegistrarUsuarioComSucesso(object payload)
    {
        var response = await _client.PostAsJsonAsync(EndpointUsers, payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created,
            "o primeiro registro de um usuário válido deve retornar 201 Created");
    }

    private async Task Passo_TentarRegistrarUsuarioDuplicado(object payload)
    {
        var response = await _client.PostAsJsonAsync(EndpointUsers, payload);

        // TODO: retorna BadRequest (400) ou Conflict (409)?
        response.StatusCode.Should().BeOneOf(
            [HttpStatusCode.BadRequest, HttpStatusCode.Conflict],
            "tentar criar um usuário que já existe deve falhar");
    }

    private async Task Passo_BloquearLoginComSenhaErrada(string username)
    {
        var payloadErrado = new { Username = username, Password = "SenhaErradaTotalmente" };
        var response = await _client.PostAsJsonAsync(EndpointLogin, payloadErrado);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "login com credenciais inválidas não deve gerar token");
    }

    private async Task<string> Passo_RealizarLoginComSucesso(object payload)
    {
        var response = await _client.PostAsJsonAsync(EndpointLogin, payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<TokenResponseDto>();

        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty("o token JWT é obrigatório para as próximas etapas");

        return result.AccessToken;
    }

    private async Task Passo_ValidarPerfilInicialDoJogador(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync(EndpointProfile);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var profile = await response.Content.ReadFromJsonAsync<UserProfileDTO>();
        profile.Should().NotBeNull();

        using (new AssertionScope())
        {
            profile!.RankPoints.Should().Be(0, "Usuário deve ser criado com 0 pontos.");
            profile.Wins.Should().Be(0, "Usuário deve ser criado com 0 vitórias (espertinho).");
            profile.Losses.Should().Be(0, "Usuário deve ser criado com 0 derrotas (trapaceiro).");
        }
    }

    #endregion
}