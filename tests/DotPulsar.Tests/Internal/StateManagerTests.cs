/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace DotPulsar.Tests.Internal;

using DotPulsar.Internal;

[Trait("Category", "Unit")]
public class StateManagerTests
{
    [Theory]
    [InlineData(ProducerState.Connected, ProducerState.Connected, ProducerState.Connected)]
    [InlineData(ProducerState.Connected, ProducerState.Disconnected, ProducerState.Connected)]
    [InlineData(ProducerState.Connected, ProducerState.Closed, ProducerState.Connected)]
    [InlineData(ProducerState.Disconnected, ProducerState.Connected, ProducerState.Disconnected)]
    [InlineData(ProducerState.Disconnected, ProducerState.Disconnected, ProducerState.Disconnected)]
    [InlineData(ProducerState.Disconnected, ProducerState.Closed, ProducerState.Disconnected)]
    [InlineData(ProducerState.Closed, ProducerState.Connected, ProducerState.Closed)]
    [InlineData(ProducerState.Closed, ProducerState.Disconnected, ProducerState.Closed)]
    [InlineData(ProducerState.Closed, ProducerState.Closed, ProducerState.Closed)]
    public void SetState_GivenNewState_ShouldReturnFormerState(ProducerState initialState, ProducerState newState, ProducerState expected)
    {
        //Arrange
        var uut = new StateManager<ProducerState>(initialState, ProducerState.Closed);

        //Act
        var actual = uut.SetState(newState);

        //Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(ProducerState.Connected)]
    [InlineData(ProducerState.Disconnected)]
    [InlineData(ProducerState.Closed)]
    public void SetState_GivenStateIsFinal_ShouldNotChangeState(ProducerState newState)
    {
        //Arrange
        var uut = new StateManager<ProducerState>(ProducerState.Closed, ProducerState.Closed);

        //Act
        _ = uut.SetState(newState);

        //Assert
        uut.CurrentState.Should().Be(ProducerState.Closed);
    }

    [Theory]
    [InlineData(ProducerState.Connected, ProducerState.Disconnected)]
    [InlineData(ProducerState.Connected, ProducerState.Closed)]
    [InlineData(ProducerState.Disconnected, ProducerState.Connected)]
    [InlineData(ProducerState.Disconnected, ProducerState.Closed)]
    public void SetState_GivenStateIsChangedToWanted_ShouldCompleteTask(ProducerState initialState, ProducerState newState)
    {
        //Arrange
        var uut = new StateManager<ProducerState>(initialState, ProducerState.Closed);
        var task = uut.StateChangedTo(newState, default);

        //Act
        _ = uut.SetState(newState);

        //Assert
        task.IsCompleted.Should().BeTrue();
    }

    [Theory]
    [InlineData(ProducerState.Connected, ProducerState.Disconnected)]
    [InlineData(ProducerState.Connected, ProducerState.Closed)]
    [InlineData(ProducerState.Disconnected, ProducerState.Connected)]
    [InlineData(ProducerState.Disconnected, ProducerState.Closed)]
    public void SetState_GivenStateIsChangedFromWanted_ShouldCompleteTask(ProducerState initialState, ProducerState newState)
    {
        //Arrange
        var uut = new StateManager<ProducerState>(initialState, ProducerState.Closed);
        var task = uut.StateChangedFrom(initialState, default);

        //Act
        _ = uut.SetState(newState);

        //Assert
        task.IsCompleted.Should().BeTrue();
    }

    [Theory]
    [InlineData(ProducerState.Connected)]
    [InlineData(ProducerState.Disconnected)]
    [InlineData(ProducerState.Closed)]
    public void StateChangedTo_GivenStateIsAlreadyWanted_ShouldCompleteTask(ProducerState state)
    {
        //Arrange
        var uut = new StateManager<ProducerState>(state, ProducerState.Closed);

        //Act
        var task = uut.StateChangedTo(state, default);

        //Assert
        Assert.True(task.IsCompleted);
    }

    [Theory]
    [InlineData(ProducerState.Connected, ProducerState.Disconnected)]
    [InlineData(ProducerState.Connected, ProducerState.Closed)]
    [InlineData(ProducerState.Disconnected, ProducerState.Connected)]
    [InlineData(ProducerState.Disconnected, ProducerState.Closed)]
    public void StateChangedTo_GivenStateIsNotWanted_ShouldNotCompleteTask(ProducerState initialState, ProducerState wantedState)
    {
        //Arrange
        var uut = new StateManager<ProducerState>(initialState, ProducerState.Closed);

        //Act
        var task = uut.StateChangedTo(wantedState, default);

        //Assert
        task.IsCompleted.Should().BeFalse();
    }

    [Theory]
    [InlineData(ProducerState.Connected)]
    [InlineData(ProducerState.Disconnected)]
    public void StateChangedTo_GivenStateIsFinal_ShouldCompleteTask(ProducerState state)
    {
        //Arrange
        var uut = new StateManager<ProducerState>(ProducerState.Closed, ProducerState.Closed);

        //Act
        var task = uut.StateChangedTo(state, default);

        //Assert
        task.IsCompleted.Should().BeTrue();
    }

    [Theory]
    [InlineData(ProducerState.Connected)]
    [InlineData(ProducerState.Disconnected)]
    public void StateChangedFrom_GivenStateHasNotChanged_ShouldNotCompleteTask(ProducerState state)
    {
        //Arrange
        var uut = new StateManager<ProducerState>(state, ProducerState.Closed);

        //Act
        var task = uut.StateChangedFrom(state, default);

        //Assert
        task.IsCompleted.Should().BeFalse();
    }

    [Theory]
    [InlineData(ProducerState.Connected, ProducerState.Disconnected)]
    [InlineData(ProducerState.Connected, ProducerState.Closed)]
    [InlineData(ProducerState.Disconnected, ProducerState.Connected)]
    [InlineData(ProducerState.Disconnected, ProducerState.Closed)]
    public void StateChangedFrom_GivenStateHasChanged_ShouldCompleteTask(ProducerState initialState, ProducerState fromState)
    {
        //Arrange
        var uut = new StateManager<ProducerState>(initialState, ProducerState.Closed);

        //Act
        var task = uut.StateChangedFrom(fromState, default);

        //Assert
        task.IsCompleted.Should().BeTrue();
    }

    [Theory]
    [InlineData(ProducerState.Connected)]
    [InlineData(ProducerState.Disconnected)]
    [InlineData(ProducerState.Closed)]
    public void StateChangedFrom_GivenStateIsFinal_ShouldCompleteTask(ProducerState state)
    {
        //Arrange
        var uut = new StateManager<ProducerState>(ProducerState.Closed, ProducerState.Closed);

        //Act
        var task = uut.StateChangedFrom(state, default);

        //Assert
        task.IsCompleted.Should().BeTrue();
    }

    [Theory]
    [InlineData(ProducerState.Connected, ProducerState.Disconnected)]
    [InlineData(ProducerState.Disconnected, ProducerState.Connected)]
    public void SetState_GivenStateIsChangeToFinalState_ShouldCompleteTask(ProducerState initialState, ProducerState wantedState)
    {
        //Arrange
        var uut = new StateManager<ProducerState>(initialState, ProducerState.Closed);

        //Act
        var task = uut.StateChangedTo(wantedState, default);
        _ = uut.SetState(ProducerState.Closed);

        //Assert
        task.IsCompleted.Should().BeTrue();
    }

    [Theory]
    [InlineData(ProducerState.Connected, ProducerState.Disconnected, ProducerState.Closed)]
    [InlineData(ProducerState.Disconnected, ProducerState.Connected, ProducerState.Closed)]
    public void SetState_GivenStateIsChangedToNotWanted_ShouldNotCompleteTask(ProducerState initialState, ProducerState newState, ProducerState wantedState)
    {
        //Arrange
        var uut = new StateManager<ProducerState>(initialState, ProducerState.Closed);

        //Act
        var task = uut.StateChangedTo(wantedState, default);
        _ = uut.SetState(newState);

        //Assert
        task.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task Cancellation_WhenTokenIsCanceledWhileWaiting_ShouldThrowException()
    {
        //Arrange
        var uut = new StateManager<ProducerState>(ProducerState.Connected, ProducerState.Closed);
        using var cts = new CancellationTokenSource();

        //Act
        var task = uut.StateChangedFrom(ProducerState.Connected, cts.Token);
        cts.Cancel();
        var exception = await Record.ExceptionAsync(() => task.AsTask()); // xUnit can't record ValueTask yet

        //Assert
        exception.Should().BeOfType<TaskCanceledException>();
    }

    [Fact]
    public async Task Cancellation_GivenCanceledToken_ShouldThrowException()
    {
        //Arrange
        var uut = new StateManager<ProducerState>(ProducerState.Connected, ProducerState.Closed);
        using var cts = new CancellationTokenSource();

        //Act
        cts.Cancel();
        var task = uut.StateChangedFrom(ProducerState.Connected, cts.Token);
        var exception = await Record.ExceptionAsync(() => task.AsTask()); // xUnit can't record ValueTask yet

        //Assert
        exception.Should().BeOfType<TaskCanceledException>();
    }
}
