using Fluxor;

namespace FocusFlow.BlazorApp.Store.Auth;

public static class AuthReducers
{
    [ReducerMethod]
    public static AuthState ReduceLoginAction(AuthState state, LoginAction action) =>
        state with { IsLoading = true, Error = null };

    [ReducerMethod]
    public static AuthState ReduceLoginSuccessAction(AuthState state, LoginSuccessAction action) =>
        state with { IsLoading = false, IsAuthenticated = true, Username = action.Username, Token = action.Token, Error = null };

    [ReducerMethod]
    public static AuthState ReduceLoginFailureAction(AuthState state, LoginFailureAction action) =>
        state with { IsLoading = false, IsAuthenticated = false, Username = null, Token = null, Error = action.Error };

    [ReducerMethod]
    public static AuthState ReduceLogoutAction(AuthState state, LogoutAction action) =>
        new AuthState(); // Reset state on logout

    [ReducerMethod]
    public static AuthState ReduceRegisterAction(AuthState state, RegisterAction action) =>
        state with { IsLoading = true, Error = null };

    [ReducerMethod]
    public static AuthState ReduceRegisterSuccessAction(AuthState state, RegisterSuccessAction action) =>
        state with { IsLoading = false, Error = null };

    [ReducerMethod]
    public static AuthState ReduceRegisterFailureAction(AuthState state, RegisterFailureAction action) =>
        state with { IsLoading = false, Error = action.Error };

    [ReducerMethod]
    public static AuthState ReduceHydrateAuthStateAction(AuthState state, HydrateAuthStateAction action) =>
        state with { 
            IsAuthenticated = !string.IsNullOrEmpty(action.Token), 
            Token = action.Token, 
            Username = action.Username 
        };
}
