﻿﻿using Shouldly;
using System.Threading.Tasks;
using System.Linq;
using Xunit;

namespace Peasy.Core.Tests
{
    public class RuleBaseTests
    {
        [Fact]
        public void Valid_Rule_Is_Valid_After_Validation()
        {
            var rule = new TrueRule().Validate();
            rule.IsValid.ShouldBe(true);
        }

        [Fact]
        public async Task Valid_Rule_Is_Valid_After_Validation_Async()
        {
            var rule = await new TrueRule().ValidateAsync();
            rule.IsValid.ShouldBe(true);
        }

        [Fact]
        public void Valid_Rule_Does_Not_Contain_An_Error_Message_After_Validation()
        {
            var rule = new TrueRule().Validate();
            rule.ErrorMessage.ShouldBe(null);
        }

        [Fact]
        public async Task Valid_Rule_Does_Not_Contain_An_Error_Message_After_Validation_Async()
        {
            var rule = await new TrueRule().ValidateAsync();
            rule.ErrorMessage.ShouldBe(null);
        }

        [Fact]
        public void Invalid_Rule_Is_Invalid_After_Validation()
        {
            var rule = new FalseRule1().Validate();
            rule.IsValid.ShouldBe(false);
        }

        [Fact]
        public async Task Invalid_Rule_Is_Invalid_After_Validation_Async()
        {
            var rule = await new FalseRule1().ValidateAsync();
            rule.IsValid.ShouldBe(false);
        }

        [Fact]
        public void Invalid_Rule_Contains_An_Error_Message_After_Validation()
        {
            var rule = new FalseRule1().Validate();
            rule.ErrorMessage.ShouldBe("FalseRule1 failed validation");
        }

        [Fact]
        public async Task Invalid_Rule_Contains_An_Error_Message_After_Validation_Async()
        {
            var rule = await new FalseRule1().ValidateAsync();
            rule.ErrorMessage.ShouldBe("FalseRule1 failed validation");
        }

        [Fact]
        public void Valid_Parent_Fails_When_Successor_Fails_Validation()
        {
            var rule1 = new TrueRule().IfValidThenValidate(new FalseRule1()).Validate();
            rule1.IsValid.ShouldBe(false);
            rule1.ErrorMessage.ShouldBe("FalseRule1 failed validation");
        }

        [Fact]
        public async Task Valid_Parent_Fails_When_Successor_Fails_Validation_Async()
        {
            var rule1 = await new TrueRule().IfValidThenValidate(new FalseRule1()).ValidateAsync();
            rule1.IsValid.ShouldBe(false);
            rule1.ErrorMessage.ShouldBe("FalseRule1 failed validation");
        }

        [Fact]
        public void Successor_Does_Not_Execute_When_Parent_Fails()
        {
            var rule1 = new FalseRule1().IfValidThenValidate(new FalseRule2()).Validate();
            rule1.IsValid.ShouldBe(false);
            rule1.ErrorMessage.ShouldBe("FalseRule1 failed validation");
        }

        [Fact]
        public async Task Successor_Does_Not_Execute_When_Parent_Fails_Async()
        {
            var rule1 = await new FalseRule1().IfValidThenValidate(new FalseRule2()).ValidateAsync();
            rule1.IsValid.ShouldBe(false);
            rule1.ErrorMessage.ShouldBe("FalseRule1 failed validation");
        }

        [Fact]
        public void All_Remaining_Successors_Skip_Validation_When_First_Successor_Fails()
        {
            var rule1 = new TrueRule()
                                .IfValidThenValidate(new FalseRule1(), new FalseRule2(), new FalseRule3())
                                .Validate();
            rule1.IsValid.ShouldBe(false);
            rule1.ErrorMessage.ShouldBe("FalseRule1 failed validation");
        }

        [Fact]
        public async Task All_Remaining_Successors_Skip_Validation_When_First_Successor_Fails_Async()
        {
            var rule1 = await new TrueRule()
                                .IfValidThenValidate(new FalseRule1(), new FalseRule2(), new FalseRule3())
                                .ValidateAsync();
            rule1.IsValid.ShouldBe(false);
            rule1.ErrorMessage.ShouldBe("FalseRule1 failed validation");
        }

        [Fact]
        public void Last_Successor_Validates_When_First_Successors_Pass()
        {
            var rule1 = new TrueRule()
                                .IfValidThenValidate(new TrueRule(), new TrueRule(), new FalseRule1())
                                .Validate();
            rule1.IsValid.ShouldBe(false);
            rule1.ErrorMessage.ShouldBe("FalseRule1 failed validation");
        }

        [Fact]
        public async Task Last_Successor_Validates_When_First_Successors_Pass_Async()
        {
            var rule1 = await new TrueRule()
                                .IfValidThenValidate(new TrueRule(), new TrueRule(), new FalseRule1())
                                .ValidateAsync();
            rule1.IsValid.ShouldBe(false);
            rule1.ErrorMessage.ShouldBe("FalseRule1 failed validation");
        }

        [Fact]
        public void Last_Successors_In_Successor_Chain_Are_Skipped_When_First_Successors_Fail()
        {
            var rule = new TrueRule()
                              .IfValidThenValidate(new TrueRule(), new FalseRule1())
                              .IfValidThenValidate(new FalseRule2(), new FalseRule3())
                              .Validate();
            rule.IsValid.ShouldBe(false);
            rule.ErrorMessage.ShouldBe("FalseRule1 failed validation");
        }

        [Fact]
        public async Task Last_Successors_In_Successor_Chain_Are_Skipped_When_First_Successors_Fail_Async()
        {
            var rule = await new TrueRule()
                              .IfValidThenValidate(new TrueRule(), new FalseRule1())
                              .IfValidThenValidate(new FalseRule2(), new FalseRule3())
                              .ValidateAsync();
            rule.IsValid.ShouldBe(false);
            rule.ErrorMessage.ShouldBe("FalseRule1 failed validation");
        }

        [Fact]
        public void Last_Successor_In_Successor_Chain_Is_Skipped_When_First_Successors_Pass()
        {
            var rule = new TrueRule()
                              .IfValidThenValidate(new TrueRule(), new TrueRule())
                              .IfValidThenValidate(new FalseRule2(), new FalseRule3())
                              .Validate();
            rule.IsValid.ShouldBe(false);
            rule.ErrorMessage.ShouldBe("FalseRule2 failed validation");
        }

        [Fact]
        public async Task Last_Successor_In_Successor_Chain_Is_Skipped_When_First_Successors_Pass_Async()
        {
            var rule = await new TrueRule()
                              .IfValidThenValidate(new TrueRule(), new TrueRule())
                              .IfValidThenValidate(new FalseRule2(), new FalseRule3())
                              .ValidateAsync();
            rule.IsValid.ShouldBe(false);
            rule.ErrorMessage.ShouldBe("FalseRule2 failed validation");
        }

        [Fact]
        public void Parent_Fails_When_Last_Successor_In_Chains_Fails_Validation()
        {
            var rule = new TrueRule()
                              .IfValidThenValidate(new TrueRule(), new TrueRule())
                              .IfValidThenValidate(new TrueRule(), new FalseRule3())
                              .Validate();
            rule.IsValid.ShouldBe(false);
            rule.ErrorMessage.ShouldBe("FalseRule3 failed validation");
        }

        [Fact]
        public async Task Parent_Fails_When_Last_Successor_In_Chains_Fails_Validation_Async()
        {
            var rule = await new TrueRule()
                              .IfValidThenValidate(new TrueRule(), new TrueRule())
                              .IfValidThenValidate(new TrueRule(), new FalseRule3())
                              .ValidateAsync();
            rule.IsValid.ShouldBe(false);
            rule.ErrorMessage.ShouldBe("FalseRule3 failed validation");
        }

        [Fact]
        public void Three_Rule_Chain_Executes_Successfully()
        {
            var rule = new TrueRule()
                            .IfValidThenValidate(new TrueRule()
                                                      .IfValidThenValidate(new TrueRule())).Validate();

            rule.IsValid.ShouldBe(true);
        }

        [Fact]
        public async Task Three_Rule_Chain_Executes_Successfully_Async()
        {
            var rule = await new TrueRule()
                            .IfValidThenValidate(new TrueRule()
                                                      .IfValidThenValidate(new TrueRule())).ValidateAsync();

            rule.IsValid.ShouldBe(true);
        }

        [Fact]
        public void Three_Rule_Chain_Fail_Skips_Third_In_Chain_When_Second_Fails()
        {
            var rule = new TrueRule()
                            .IfValidThenValidate(new FalseRule1()
                                                      .IfValidThenValidate(new FalseRule2())).Validate();
            rule.IsValid.ShouldBe(false);
            rule.ErrorMessage.ShouldBe("FalseRule1 failed validation");
        }

        [Fact]
        public async Task Three_Rule_Chain_Fail_Skips_Third_In_Chain_When_Second_Fails_Async()
        {
            var rule = await new TrueRule()
                            .IfValidThenValidate(new FalseRule1()
                                                      .IfValidThenValidate(new FalseRule2())).ValidateAsync();
            rule.IsValid.ShouldBe(false);
            rule.ErrorMessage.ShouldBe("FalseRule1 failed validation");
        }

        [Fact]
        public void Three_Rule_Chain_Hits_Third_In_Chain_And_Fails_Parent()
        {
            var rule = new TrueRule()
                            .IfValidThenValidate(new TrueRule()
                                                      .IfValidThenValidate(new FalseRule1())).Validate();
            rule.IsValid.ShouldBe(false);
            rule.ErrorMessage.ShouldBe("FalseRule1 failed validation");
        }

        [Fact]
        public async Task Three_Rule_Chain_Hits_Third_In_Chain_And_Fails_Parent_Async()
        {
            var rule = await new TrueRule()
                            .IfValidThenValidate(new TrueRule()
                                                      .IfValidThenValidate(new FalseRule1())).ValidateAsync();
            rule.IsValid.ShouldBe(false);
            rule.ErrorMessage.ShouldBe("FalseRule1 failed validation");
        }

        [Fact]
        public void Invokes_IfValidThenExecute()
        {
            var output = string.Empty;
            new TrueRule()
                    .IfValidThenExecute(rule => output = "pass")
                    .Validate();

            output.ShouldBe("pass");
        }

        [Fact]
        public async Task Invokes_IfValidThenExecute_Async()
        {
            var output = string.Empty;
            await new TrueRule()
                    .IfValidThenExecute(rule => output = "pass")
                    .ValidateAsync();

            output.ShouldBe("pass");
        }

        [Fact]
        public void Does_Not_Invoke_IfValidThenExecute()
        {
            var output = string.Empty;
            new FalseRule1()
                    .IfValidThenExecute(rule => output = "pass")
                    .Validate();

            output.ShouldBe(string.Empty);
        }

        [Fact]
        public async Task Does_Not_Invoke_IfValidThenExecute_Async()
        {
            var output = string.Empty;
            await new FalseRule1()
                    .IfValidThenExecute(rule => output = "pass")
                    .ValidateAsync();

            output.ShouldBe(string.Empty);
        }

        [Fact]
        public void Invokes_IfInvalidThenExecute()
        {
            var output = string.Empty;
            new FalseRule1()
                    .IfInvalidThenExecute(rule => output = "pass")
                    .Validate();

            output.ShouldBe("pass");
        }

        [Fact]
        public async Task Invokes_IfInvalidThenExecute_Async()
        {
            var output = string.Empty;
            await new FalseRule1()
                    .IfInvalidThenExecute(rule => output = "pass")
                    .ValidateAsync();

            output.ShouldBe("pass");
        }

        [Fact]
        public void Does_Not_Invoke_IfInvalidThenExecute()
        {
            var output = string.Empty;
            new TrueRule()
                   .IfInvalidThenExecute(rule => output = "pass")
                   .Validate();

            output.ShouldBe(string.Empty);
        }

        [Fact]
        public async Task Does_Not_Invoke_IfInvalidThenExecute_Async()
        {
            var output = string.Empty;
            await new TrueRule()
                   .IfInvalidThenExecute(rule => output = "pass")
                   .ValidateAsync();

            output.ShouldBe(string.Empty);
        }

        [Fact]
        public void Successor_Invokes_IfValidThenExecute()
        {
            var output = string.Empty;
            new TrueRule()
                    .IfValidThenValidate(new TrueRule()
                                                .IfValidThenExecute(r => output = "pass")).Validate();

            output.ShouldBe("pass");
        }

        [Fact]
        public async Task Successor_Invokes_IfValidThenExecute_Async()
        {
            var output = string.Empty;
            await new TrueRule()
                    .IfValidThenValidate(new TrueRule()
                                                .IfValidThenExecute(r => output = "pass")).ValidateAsync();

            output.ShouldBe("pass");
        }

        [Fact]
        public void Successor_Does_Not_Invoke_IfValidThenExecute()
        {
            var output = string.Empty;
            new TrueRule()
                .IfValidThenValidate(new FalseRule1()
                                         .IfValidThenExecute(r => output = "pass")).Validate();

            output.ShouldBe(string.Empty);
        }

        [Fact]
        public async Task Successor_Does_Not_Invoke_IfValidThenExecute_Async()
        {
            var output = string.Empty;
            await new TrueRule()
                .IfValidThenValidate(new FalseRule1()
                                         .IfValidThenExecute(r => output = "pass")).ValidateAsync();

            output.ShouldBe(string.Empty);
        }

        [Fact]
        public void Successor_Invokes_IfInvalidThenExecute()
        {
            var output = string.Empty;
            new TrueRule()
                .IfValidThenValidate(new FalseRule1()
                                         .IfInvalidThenExecute(r => output = "pass")).Validate();

            output.ShouldBe("pass");
        }

        [Fact]
        public async Task Successor_Invokes_IfInvalidThenExecute_Async()
        {
            var output = string.Empty;
            await new TrueRule()
                .IfValidThenValidate(new FalseRule1()
                                         .IfInvalidThenExecute(r => output = "pass")).ValidateAsync();

            output.ShouldBe("pass");
        }

        [Fact]
        public void Successor_Does_Not_Invoke_IfInvalidThenExecute()
        {
            var output = string.Empty;
            new TrueRule()
                .IfValidThenValidate(new TrueRule()
                                         .IfInvalidThenExecute(r => output = "pass")).Validate();

            output.ShouldBe(string.Empty);
        }

        [Fact]
        public async Task Successor_Does_Not_Invoke_IfInvalidThenExecute_Async()
        {
            var output = string.Empty;
            await new TrueRule()
                .IfValidThenValidate(new TrueRule()
                                         .IfInvalidThenExecute(r => output = "pass")).ValidateAsync();

            output.ShouldBe(string.Empty);
        }

        [Fact]
        public void First_Valid_Rule_In_First_Successor_Chain_Should_Execute()
        {
            var output = string.Empty;
            var rule = new TrueRule()
                          .IfValidThenValidate(new TrueRule().IfValidThenExecute(r => output = "pass"), new TrueRule())
                          .IfValidThenValidate(new TrueRule(), new FalseRule3())
                          .Validate();
            output.ShouldBe("pass");
        }

        [Fact]
        public async Task First_Valid_Rule_In_First_Successor_Chain_Should_Execute_Async()
        {
            var output = string.Empty;
            var rule = await new TrueRule()
                .IfValidThenValidate(new TrueRule().IfValidThenExecute(r => output = "pass"), new TrueRule())
                .IfValidThenValidate(new TrueRule(), new FalseRule3())
                .ValidateAsync();
            output.ShouldBe("pass");
        }

        [Fact]
        public void Second_Valid_Rule_In_First_Successor_Chain_Should_Execute()
        {
            var output = string.Empty;
            var rule = new TrueRule()
                .IfValidThenValidate(new TrueRule(), new TrueRule().IfValidThenExecute(r => output = "pass"))
                .IfValidThenValidate(new TrueRule(), new FalseRule3())
                .Validate();
            output.ShouldBe("pass");
        }

        [Fact]
        public async Task Second_Valid_Rule_In_First_Successor_Chain_Should_Execute_Async()
        {
            var output = string.Empty;
            var rule = await new TrueRule()
                .IfValidThenValidate(new TrueRule(), new TrueRule().IfValidThenExecute(r => output = "pass"))
                .IfValidThenValidate(new TrueRule(), new FalseRule3())
                .ValidateAsync();
            output.ShouldBe("pass");
        }

        [Fact]
        public void First_Valid_Rule_In_Second_Successor_Chain_Should_Execute()
        {
            var output = string.Empty;
            var rule = new TrueRule()
                .IfValidThenValidate(new TrueRule(), new TrueRule())
                .IfValidThenValidate(new TrueRule().IfValidThenExecute(r => output = "pass"), new FalseRule3())
                .Validate();
            output.ShouldBe("pass");
        }

        [Fact]
        public async Task First_Valid_Rule_In_Second_Successor_Chain_Should_Execute_Async()
        {
            var output = string.Empty;
            var rule = await new TrueRule()
                .IfValidThenValidate(new TrueRule(), new TrueRule())
                .IfValidThenValidate(new TrueRule().IfValidThenExecute(r => output = "pass"), new FalseRule3())
                .ValidateAsync();
            output.ShouldBe("pass");
        }

        [Fact]
        public void Second_Invalid_Rule_In_Second_Successor_Chain_Should_Execute()
        {
            var output = string.Empty;
            var rule = new TrueRule()
                .IfValidThenValidate(new TrueRule(), new TrueRule())
                .IfValidThenValidate(new TrueRule(), new FalseRule3().IfInvalidThenExecute(r => output = "pass"))
                .Validate();
            output.ShouldBe("pass");
        }

        [Fact]
        public async Task Second_Invalid_Rule_In_Second_Successor_Chain_Should_Execute_Async()
        {
            var output = string.Empty;
            var rule = await new TrueRule()
                .IfValidThenValidate(new TrueRule(), new TrueRule())
                .IfValidThenValidate(new TrueRule(), new FalseRule3().IfInvalidThenExecute(r => output = "pass"))
                .ValidateAsync();
            output.ShouldBe("pass");
        }

        [Fact]
        public void First_Valid_Rule_In_Second_Successor_Chain_Should_Execute_But_Third_False_Rule_Should_Not()
        {
            var output = string.Empty;
            var output2 = string.Empty;
            var rule = new TrueRule()
                              .IfValidThenValidate(new TrueRule().IfValidThenExecute(r => output = "pass"), new TrueRule())
                              .IfValidThenValidate(new FalseRule2().IfValidThenExecute(r => output2 = "pass"), new FalseRule3())
                              .Validate();
            output.ShouldBe("pass");
            output2.ShouldBe(string.Empty);
            rule.ErrorMessage.ShouldBe("FalseRule2 failed validation");
        }

        [Fact]
        public async Task First_Valid_Rule_In_Second_Successor_Chain_Should_Execute_But_Third_False_Rule_Should_Not_Async()
        {
            var output = string.Empty;
            var output2 = string.Empty;
            var rule = await new TrueRule()
                              .IfValidThenValidate(new TrueRule().IfValidThenExecute(r => output = "pass"), new TrueRule())
                              .IfValidThenValidate(new FalseRule2().IfValidThenExecute(r => output2 = "pass"), new FalseRule3())
                              .ValidateAsync();
            output.ShouldBe("pass");
            output2.ShouldBe(string.Empty);
            rule.ErrorMessage.ShouldBe("FalseRule2 failed validation");
        }

        [Fact]
        public void The_Correct_Association_Is_Set_As_A_Result_Of_Failed_Successor()
        {
            var rule = new TrueRule("Foo")
                .IfValidThenValidate(new TrueRule(), new FalseRuleWithAssociation("Address"));
            rule.Validate();
            rule.Association.ShouldBe("Address");
            rule.ErrorMessage.ShouldBe("Address failed validation");
        }

        [Fact]
        public async Task The_Correct_Association_Is_Set_As_A_Result_Of_Failed_Successor_Async()
        {
            var rule = new TrueRule("Foo")
                .IfValidThenValidate(new TrueRule(), new FalseRuleWithAssociation("Address"));
            await rule.ValidateAsync();
            rule.Association.ShouldBe("Address");
            rule.ErrorMessage.ShouldBe("Address failed validation");
        }

        [Fact]
        public void Allows_access_to_successor_rules_via_IRulesContainer_interface()
        {
            var rule = new TrueRule()
                .IfValidThenValidate(new TrueRule(), new FalseRule2())
                .IfValidThenValidate
                (
                    new TrueRule().IfValidThenValidate(new FalseRule1()),
                    new FalseRule3()
                );

            rule.GetSuccessors().Count().ShouldBe(2);

            var firstSuccessor = rule.GetSuccessors().First();
            firstSuccessor.Rules.Count().ShouldBe(2);
            firstSuccessor.Rules.First().ShouldBeOfType<TrueRule>();
            firstSuccessor.Rules.Second().ShouldBeOfType<FalseRule2>();

            var secondSuccessor = rule.GetSuccessors().Second();
            secondSuccessor.Rules.Count().ShouldBe(2);
            secondSuccessor.Rules.First().ShouldBeOfType<TrueRule>();
            secondSuccessor.Rules.First().GetSuccessors().Count().ShouldBe(1);
            secondSuccessor.Rules.First().GetSuccessors().First().Rules.First().ShouldBeOfType<FalseRule1>();
            secondSuccessor.Rules.Second().ShouldBeOfType<FalseRule3>();
        }
    }

}
