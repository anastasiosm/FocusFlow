using Fluxor;

namespace FocusFlow.BlazorApp.Features.Auth.Register.Store;

public static class RegisterReducers
{
    [ReducerMethod]
    public static RegisterState ReduceRegisterAction(RegisterState state, RegisterAction action) =>
        state with { IsLoading = true, Error = null };

    [ReducerMethod]
    public static RegisterState ReduceRegisterSuccessAction(RegisterState state, RegisterSuccessAction action) =>
        state with { IsLoading = false, Error = null };

    [ReducerMethod]
    public static RegisterState ReduceRegisterFailureAction(RegisterState state, RegisterFailureAction action) =>
        state with { IsLoading = false, Error = action.Error };
}