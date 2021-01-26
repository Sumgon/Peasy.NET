﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Peasy
{
    /// <summary>
    /// Defines a base command responsible for the execution of a logical unit of work.
    /// </summary>
    public abstract class Command : ICommand, IRulesContainer, ISupportValidation
    {
        /// <inheritdoc cref="ICommand.Execute"/>
        public virtual ExecutionResult Execute()
        {
            OnInitialization();

            var validationResults = OnValidate().ToArray();

            if (validationResults.Any()) return OnFailedExecution(validationResults);

            try
            {
                OnExecute();
            }
            catch (PeasyException ex)
            {
                return OnPeasyExceptionHandled(ex);
            }

            return OnSuccessfulExecution();
        }

        /// <inheritdoc cref="ICommand.ExecuteAsync"/>
        public virtual async Task<ExecutionResult> ExecuteAsync()
        {
            await OnInitializationAsync();

            var validationResults = (await OnValidateAsync()).ToArray();

            if (validationResults.Any()) return OnFailedExecution(validationResults);

            try
            {
                await OnExecuteAsync();
            }
            catch (PeasyException ex)
            {
                return OnPeasyExceptionHandled(ex);
            }

            return OnSuccessfulExecution();
        }

        /// <summary>
        /// Performs initialization logic.
        /// </summary>
        /// <remarks>
        /// <para>Invoked within the execution pipeline triggered by <see cref="Execute"/>.</para>
        /// <para>Override this method to perform initialization logic before rule executions occur.</para>
        /// </remarks>
        protected virtual void OnInitialization() { }

        /// <summary>
        /// Performs initialization logic.
        /// </summary>
        /// <remarks>
        /// <para>Invoked within the execution pipeline triggered by <see cref="ExecuteAsync"/>.</para>
        /// <para>Override this method to perform initialization logic before rule executions occur.</para>
        /// </remarks>
        /// <returns>An awaitable task.</returns>
        protected virtual Task OnInitializationAsync()
        {
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Performs rule validations.
        /// </summary>
        /// <remarks>
        /// <para>Invoked within the execution pipeline triggered by <see cref="Execute"/>.</para>
        /// <para>Override this method to manipulate how rules are invoked.</para>
        /// </remarks>
        /// <returns>A potential list of errors resulting from rule executions.</returns>
        protected virtual IEnumerable<ValidationResult> OnValidate()
        {
            return OnGetRules().GetValidationResults();
        }

        /// <summary>
        /// Performs rule validations.
        /// </summary>
        /// <remarks>
        /// <para>Invoked within the execution pipeline triggered by <see cref="ExecuteAsync"/>.</para>
        /// <para>Override this method to manipulate how rules are invoked.</para>
        /// </remarks>
        /// <returns>A potential awaitable list of errors resulting from rule executions.</returns>
        protected virtual async Task<IEnumerable<ValidationResult>> OnValidateAsync()
        {
            var rules = await OnGetRulesAsync();
            return await rules.GetValidationResultsAsync();
        }

        /// <summary>
        /// Executes application logic.
        /// </summary>
        /// <remarks>
        /// <para>Invoked within the execution pipeline triggered by <see cref="Execute"/> if all rule validations are successful.</para>
        /// <para>Override this method to perform custom application logic.</para>
        /// </remarks>
        protected virtual void OnExecute() { }

        /// <summary>
        /// Executes application logic.
        /// </summary>
        /// <remarks>
        /// <para>Invoked within the execution pipeline triggered by <see cref="ExecuteAsync"/> if all rule validations are successful.</para>
        /// <para>Override this method to perform custom application logic.</para>
        /// </remarks>
        /// <returns>An awaitable task.</returns>
        protected virtual Task OnExecuteAsync()
        {
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Composes a list of business and validation rules to execute.
        /// </summary>
        /// <remarks>
        /// <para>Invoked within the execution pipeline triggered by <see cref="Execute"/>.</para>
        /// <para>Override this method to supply custom business rules to execute.</para>
        /// </remarks>
        /// <returns>A list of business and validation rules.</returns>
        protected virtual IEnumerable<IRule> OnGetRules()
        {
            return Enumerable.Empty<IRule>();
        }

        /// <summary>
        /// Composes a list of business and validation rules to execute.
        /// </summary>
        /// <remarks>
        /// <para>Invoked within the execution pipeline triggered by <see cref="ExecuteAsync"/>.</para>
        /// <para>Override this method to supply custom business rules to execute.</para>
        /// </remarks>
        /// <returns>An awaitable list of business and validation rules.</returns>
        protected virtual Task<IEnumerable<IRule>> OnGetRulesAsync()
        {
            return Task.FromResult(Enumerable.Empty<IRule>());
        }

        /// <summary>
        /// Invoked when an exception of type <see cref="PeasyException"/> is handled.
        /// </summary>
        /// <remarks>
        /// Override this method to return a custom execution result of type <see cref="ExecutionResult"/> or to further manipulate it when a <see cref="PeasyException"/> is handled.
        /// </remarks>
        /// <returns>A failed execution result.</returns>
        protected virtual ExecutionResult OnPeasyExceptionHandled(PeasyException exception)
        {
            return OnFailedExecution(new[] { new ValidationResult(exception.Message) });
        }

        /// <summary>
        /// Invoked when any of the rules in the pipeline fail execution.
        /// </summary>
        /// <remarks>
        /// <para>Override this method to return a custom execution result of type <see cref="ExecutionResult"/> or to further manipulate it.</para>
        /// <para>Override this method to return custom validation results of type <see cref="ValidationResult"/> or to further manipulate them.</para>
        /// </remarks>
        /// <returns>A failed execution result.</returns>
        protected virtual ExecutionResult OnFailedExecution(IEnumerable<ValidationResult> validationResults)
        {
            return new ExecutionResult { Success = false, Errors = validationResults };
        }

        /// <summary>
        /// Invoked when the successful execution of the command pipeline is complete.
        /// </summary>
        /// <remarks>
        /// Override this method to return a custom execution result of type <see cref="ExecutionResult"/> or to further manipulate it.
        /// </remarks>
        /// <returns>A successful execution result.</returns>
        protected virtual ExecutionResult OnSuccessfulExecution()
        {
            return new ExecutionResult { Success = true };
        }

        /// <inheritdoc cref="IRulesContainer.GetRulesAsync"/>
        public Task<IEnumerable<IRule>> GetRulesAsync()
        {
            return OnGetRulesAsync();
        }

        /// <inheritdoc cref="IRulesContainer.GetRules"/>
        public IEnumerable<IRule> GetRules()
        {
            return OnGetRules();
        }

        /// <inheritdoc cref="ISupportValidation.Validate"/>
        public IEnumerable<ValidationResult> Validate()
        {
            return OnValidate();
        }

        /// <inheritdoc cref="ISupportValidation.ValidateAsync"/>
        public Task<IEnumerable<ValidationResult>> ValidateAsync()
        {
            return OnValidateAsync();
        }
    }

    /// <summary>
    /// Defines a base command responsible for the execution of a logical unit of work.
    /// </summary>
    public abstract class Command<T> : ICommand<T>, IRulesContainer, ISupportValidation
    {
        /// <inheritdoc cref="ICommand{T}.Execute"/>
        public virtual ExecutionResult<T> Execute()
        {
            OnInitialization();

            var validationResults = OnValidate().ToArray();

            if (validationResults.Any()) return OnFailedExecution(validationResults);

            T result;
            try
            {
                result = OnExecute();
            }
            catch (PeasyException ex)
            {
                return OnPeasyExceptionHandled(ex);
            }

            return OnSuccessfulExecution(result);
        }

        /// <inheritdoc cref="ICommand{T}.ExecuteAsync"/>
        public virtual async Task<ExecutionResult<T>> ExecuteAsync()
        {
            await OnInitializationAsync();

            var validationResults = (await OnValidateAsync()).ToArray();

            if (validationResults.Any()) return OnFailedExecution(validationResults);

            T result;
            try
            {
                result = await OnExecuteAsync();
            }
            catch (PeasyException ex)
            {
                return OnPeasyExceptionHandled(ex);
            }

            return OnSuccessfulExecution(result);
        }

        /// <summary>
        /// Performs initialization logic.
        /// </summary>
        /// <remarks>
        /// <para>Invoked within the execution pipeline triggered by <see cref="Execute"/>.</para>
        /// <para>Override this method to perform initialization logic before rule executions occur.</para>
        /// </remarks>
        protected virtual void OnInitialization() { }

        /// <summary>
        /// Performs initialization logic.
        /// </summary>
        /// <remarks>
        /// <para>Invoked within the execution pipeline triggered by <see cref="ExecuteAsync"/>.</para>
        /// <para>Override this method to perform initialization logic before rule executions occur.</para>
        /// </remarks>
        /// <returns>An awaitable task.</returns>
        protected virtual Task OnInitializationAsync()
        {
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Performs rule validations.
        /// </summary>
        /// <remarks>
        /// <para>Invoked within the execution pipeline triggered by <see cref="Execute"/>.</para>
        /// <para>Override this method to manipulate how rules are invoked.</para>
        /// </remarks>
        /// <returns>A potential list of errors resulting from rule executions.</returns>
        protected virtual IEnumerable<ValidationResult> OnValidate()
        {
            return OnGetRules().GetValidationResults();
        }

        /// <summary>
        /// Performs rule validations.
        /// </summary>
        /// <remarks>
        /// <para>Invoked within the execution pipeline triggered by <see cref="ExecuteAsync"/>.</para>
        /// <para>Override this method to manipulate how rules are invoked.</para>
        /// </remarks>
        /// <returns>A potential awaitable list of errors resulting from rule executions.</returns>
        protected virtual async Task<IEnumerable<ValidationResult>> OnValidateAsync()
        {
            var rules = await OnGetRulesAsync();
            return await rules.GetValidationResultsAsync();
        }

        /// <summary>
        /// Executes application logic.
        /// </summary>
        /// <remarks>
        /// <para>Invoked within the execution pipeline triggered by <see cref="Execute"/> if all rule validations are successful.</para>
        /// <para>Override this method to perform custom application logic.</para>
        /// </remarks>
        /// <returns>A resource of type <typeparamref name="T"/> resulting from successful command execution.</returns>
        protected virtual T OnExecute()
        {
            return default(T);
        }

        /// <summary>
        /// Executes application logic.
        /// </summary>
        /// <remarks>
        /// <para>Invoked within the execution pipeline triggered by <see cref="ExecuteAsync"/> if all rule validations are successful.</para>
        /// <para>Override this method to perform custom application logic.</para>
        /// </remarks>
        /// <returns>An awaitable resource of type <typeparamref name="T"/> resulting from successful command execution.</returns>
        protected virtual Task<T> OnExecuteAsync()
        {
            return Task.FromResult(default(T));
        }

        /// <summary>
        /// Composes a list of business and validation rules to execute.
        /// </summary>
        /// <remarks>
        /// <para>Invoked within the execution pipeline triggered by <see cref="Execute"/>.</para>
        /// <para>Override this method to supply custom business rules to execute.</para>
        /// </remarks>
        /// <returns>A list of business and validation rules.</returns>
        protected virtual IEnumerable<IRule> OnGetRules()
        {
            return Enumerable.Empty<IRule>();
        }

        /// <summary>
        /// Composes a list of business and validation rules to execute.
        /// </summary>
        /// <remarks>
        /// <para>Invoked within the execution pipeline triggered by <see cref="ExecuteAsync"/>.</para>
        /// <para>Override this method to supply custom business rules to execute.</para>
        /// </remarks>
        /// <returns>An awaitable list of business and validation rules.</returns>
        protected virtual Task<IEnumerable<IRule>> OnGetRulesAsync()
        {
            return Task.FromResult(Enumerable.Empty<IRule>());
        }

        /// <summary>
        /// Invoked when an exception of type <see cref="PeasyException"/> is handled.
        /// </summary>
        /// <remarks>
        /// Override this method to return a custom execution result of type <see cref="ExecutionResult{T}"/> or to further manipulate it when a <see cref="PeasyException"/> is handled.
        /// </remarks>
        /// <returns>A failed execution result.</returns>
        protected virtual ExecutionResult<T> OnPeasyExceptionHandled(PeasyException exception)
        {
            return OnFailedExecution(new[] { new ValidationResult(exception.Message) });
        }

        /// <summary>
        /// Invoked when any of the rules in the pipeline fail execution.
        /// </summary>
        /// <remarks>
        /// <para>Override this method to return a custom execution result of type <see cref="ExecutionResult{T}"/> or to further manipulate it.</para>
        /// <para>Override this method to return custom validation results of type <see cref="ValidationResult"/> or to further manipulate them.</para>
        /// </remarks>
        /// <returns>A failed execution result.</returns>
        protected virtual ExecutionResult<T> OnFailedExecution(ValidationResult[] validationResults)
        {
            return new ExecutionResult<T> { Success = false, Errors = validationResults };
        }

        /// <summary>
        /// Invoked when the successful execution of the command pipeline is complete.
        /// </summary>
        /// <remarks>
        /// Override this method to return a custom execution result of type <see cref="ExecutionResult{T}"/> or to further manipulate it.
        /// </remarks>
        /// <returns>A successful execution result.</returns>
        protected virtual ExecutionResult<T> OnSuccessfulExecution(T value)
        {
            return new ExecutionResult<T> { Success = true, Value = value };
        }

        /// <inheritdoc cref="IRulesContainer.GetRulesAsync"/>
        public Task<IEnumerable<IRule>> GetRulesAsync()
        {
            return OnGetRulesAsync();
        }

        /// <inheritdoc cref="IRulesContainer.GetRules"/>
        public IEnumerable<IRule> GetRules()
        {
            return OnGetRules();
        }

        /// <inheritdoc cref="ISupportValidation.Validate"/>
        public IEnumerable<ValidationResult> Validate()
        {
            return OnValidate();
        }

        /// <inheritdoc cref="ISupportValidation.ValidateAsync"/>
        public Task<IEnumerable<ValidationResult>> ValidateAsync()
        {
            return OnValidateAsync();
        }
    }
}
