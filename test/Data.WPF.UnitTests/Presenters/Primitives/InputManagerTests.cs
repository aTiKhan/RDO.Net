﻿using DevZest.Data;
using DevZest.Data.Views;
using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace DevZest.Data.Presenters.Primitives
{
    [TestClass]
    public class InputManagerTests
    {
        [TestMethod]
        public void InputManager_RowInput()
        {
            var dataSet = DataSetMock.ProductCategories(3, false);
            var _ = dataSet._;
            RowBinding<TextBox> textBox = null;
            var inputManager = dataSet.CreateInputManager((builder) =>
            {
                textBox = _.ParentProductCategoryID.BindToTextBox();
                builder.GridColumns("100").GridRows("100").AddBinding(0, 0, textBox);
            });

            var element = textBox[inputManager.CurrentRow];
            Assert.IsTrue(string.IsNullOrEmpty(element.Text));
            Assert.IsNull(inputManager.RowValidation.GetFlushingError(element));
            {
                var errors = System.Windows.Controls.Validation.GetErrors(element);
                Assert.AreEqual(0, errors.Count);
            }

            element.Text = "A";
            Assert.IsNotNull(inputManager.RowValidation.GetFlushingError(element));
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[inputManager.CurrentRow]);
                Assert.AreEqual(1, errors.Count);
                Assert.AreEqual(inputManager.RowValidation.GetFlushingError(element), errors[0].ErrorContent);
            }

            element.Text = "100";
            Assert.IsNull(inputManager.RowValidation.GetFlushingError(element));
            Assert.AreEqual(100, dataSet._.ParentProductCategoryID[inputManager.CurrentRow.DataRow]);
            {
                var errors = System.Windows.Controls.Validation.GetErrors(element);
                Assert.AreEqual(0, errors.Count);
            }
        }

        [TestMethod]
        public void InputManager_Scalar()
        {
            IScalars valueChangedScalars = null;
            var scalarContainer = ScalarContainerMock.New(x =>
            {
                valueChangedScalars = x;
            });

            var scalar = scalarContainer.CreateNew<int>(0)
                .AddValidator(x =>
                {
                    return x > 5 ? "Value cannot be greater than 5." : null;
                });

            var valueChanged = scalar.SetValue(4);
            Assert.IsTrue(valueChanged);
            Assert.AreEqual(scalar, valueChangedScalars);
        }

        [TestMethod]
        public void InputManager_ScalarInput()
        {
            var dataSet = DataSetMock.ProductCategories(3, false);
            var _ = dataSet._;
            Scalar<Int32> scalar = ScalarContainerMock.New().CreateNew<int>().AddValidator(x =>
            {
                return x > 5 ? "Value cannot be greater than 5." : null;
            });
            ScalarBinding<TextBox> textBox = null;
            RowBinding<TextBlock> textBlock = null;
            var inputManager = dataSet.CreateInputManager((builder) =>
            {
                textBox = scalar.BindToTextBox();
                textBlock = _.Name.BindToTextBlock(); // to avoid empty RowRange
                builder.GridColumns("100").GridRows("100", "100").AddBinding(0, 0, textBox).AddBinding(0, 1, textBlock);
            }).WithScalars(new Scalar[] { scalar });

            Assert.AreEqual("0", textBox[0].Text);
            Assert.IsNull(inputManager.ScalarValidation.GetFlushingError(textBox[0]));
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[0]);
                Assert.AreEqual(0, errors.Count);
            }

            textBox[0].Text = "A";
            Assert.IsNotNull(inputManager.ScalarValidation.GetFlushingError(textBox[0]));
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[0]);
                Assert.AreEqual(1, errors.Count);
                Assert.AreEqual(inputManager.ScalarValidation.GetFlushingError(textBox[0]), errors[0].ErrorContent);
            }

            textBox[0].Text = "4";
            Assert.IsNull(inputManager.ScalarValidation.GetFlushingError(textBox[0]));
            Assert.AreEqual(4, scalar.GetValue());
            Assert.IsNull(inputManager.ScalarValidation.GetFlushingError(textBox[0]));
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[0]);
                Assert.AreEqual(0, errors.Count);
            }

            inputManager.ScalarValidation.UpdateProgress(textBox.Input, true, true);
            textBox[0].Text = "6";
            Assert.AreEqual("6", textBox[0].Text);
            Assert.AreEqual(1, inputManager.ScalarValidation.Errors.Count);
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[0]);
                Assert.AreEqual(1, errors.Count);
                Assert.AreEqual(inputManager.ScalarValidation.Errors, errors[0].ErrorContent);
            }
        }

        [TestMethod]
        public void InputManager_Validate_Progress()
        {
            var dataSet = DataSet<ProductCategory>.Create();
            var _ = dataSet._;
            dataSet.Add(new DataRow());

            RowBinding<TextBox> textBox = null;
            var inputManager = dataSet.CreateInputManager(builder =>
            {
                textBox = _.Name.BindToTextBox();
                builder.GridColumns("100").GridRows("100").AddBinding(0, 0, textBox);
            });

            var currentRow = inputManager.CurrentRow;
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[currentRow]);
                Assert.AreEqual(0, errors.Count);
            }

            textBox[currentRow].Text = "some name";
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[currentRow]);
                Assert.AreEqual(0, errors.Count);
            }

            inputManager.RowValidation.UpdateProgress(textBox.Input, true, true);

            textBox[currentRow].Text = null;
            Assert.AreEqual(string.Empty, textBox[currentRow].Text);
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[currentRow]);
                Assert.AreEqual(1, errors.Count);
                var error = (DataValidationError)errors[0].ErrorContent;
                Assert.AreEqual(_.Name, error.Source);
            }

            textBox[currentRow].Text = "some other name";
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[currentRow]);
                Assert.AreEqual(0, errors.Count);
            }
        }

        [TestMethod]
        public void InputManager_Validate_Implicit()
        {
            var dataSet = DataSet<ProductCategory>.Create();
            var _ = dataSet._;
            dataSet.Add(new DataRow());

            RowBinding<TextBox> textBox = null;
            var inputManager = dataSet.CreateInputManager(builder =>
            {
                textBox = _.Name.BindToTextBox();
                builder.GridColumns("100").GridRows("100").AddBinding(0, 0, textBox).WithRowValidationMode(ValidationMode.Implicit);
            });

            var currentRow = inputManager.CurrentRow;
            Assert.IsNull(_.Name[0]);
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[currentRow]);
                Assert.AreEqual(1, errors.Count);
                var error = (DataValidationError)errors[0].ErrorContent;
                Assert.AreEqual(_.Name, error.Source);
            }

            textBox[currentRow].Text = "some name";
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[currentRow]);
                Assert.AreEqual(0, errors.Count);
            }
        }

        private const string BAD_NAME = "Bad Name";

        [TestMethod]
        public void InputManager_AsyncValidators_input_error_to_invalid_to_valid()
        {
            RunInWpfSyncContext(InputManager_AsyncValidators_input_error_to_invalid_to_valid_Async);
        }

        private async Task InputManager_AsyncValidators_input_error_to_invalid_to_valid_Async()
        {
            var dataSet = DataSet<ProductCategory>.Create();
            var _ = dataSet._;
            dataSet.Add(new DataRow());

            RowBinding<TextBox> textBox = null;
            var inputManager = dataSet.CreateInputManager(builder =>
            {
                textBox = _.Name.BindToTextBox();

                builder.GridColumns("100").GridRows("100")
                    .AddBinding(0, 0, textBox).WithRowValidationMode(ValidationMode.Implicit);

                builder.AddAsyncValidator(textBox.Input, dataRow => ValidateBadNameAsync(_.Name, dataRow));
            });

            var currentRow = inputManager.CurrentRow;
            var asyncValidator = inputManager.RowValidation.AsyncValidators[0];
            Assert.AreEqual(AsyncValidatorStatus.Inactive, asyncValidator.Status);

            textBox[currentRow].Text = BAD_NAME;
            Assert.AreEqual(AsyncValidatorStatus.Running, asyncValidator.Status);

            await asyncValidator.LastRunTask;
            Assert.AreEqual(AsyncValidatorStatus.Error, asyncValidator.Status);

            textBox[currentRow].Text = "Good Name";
            Assert.AreEqual(AsyncValidatorStatus.Running, asyncValidator.Status);

            await asyncValidator.LastRunTask;
            Assert.AreEqual(AsyncValidatorStatus.Validated, asyncValidator.Status);
        }

        private static async Task<string> ValidateBadNameAsync(_String nameColumn, DataRow dataRow)
        {
            await Task.Delay(200);
            var value = nameColumn[dataRow];
            return value == BAD_NAME ? "Bad Name" : null;
        }

        [TestMethod]
        public void InputManager_AsyncValidators_faulted()
        {
            RunInWpfSyncContext(InputManager_AsyncValidators_faulted_Async);
        }

        private async Task InputManager_AsyncValidators_faulted_Async()
        {
            var dataSet = DataSet<ProductCategory>.Create();
            var _ = dataSet._;
            dataSet.Add(new DataRow());

            RowBinding<TextBox> textBox = null;
            var inputManager = dataSet.CreateInputManager(builder =>
            {
                textBox = _.Name.BindToTextBox();

                builder.GridColumns("100").GridRows("100")
                    .AddBinding(0, 0, textBox).WithRowValidationMode(ValidationMode.Implicit);

                builder.AddAsyncValidator(textBox.Input, dataRow => ValidateFaultedAsync(dataRow));
            });

            var currentRow = inputManager.CurrentRow;
            var asyncValidator = inputManager.RowValidation.AsyncValidators[0];

            textBox[currentRow].Text = "Anything";
            Assert.AreEqual(AsyncValidatorStatus.Running, asyncValidator.Status);

            await asyncValidator.LastRunTask;
            Assert.AreEqual(AsyncValidatorStatus.Faulted, asyncValidator.Status);
            Assert.AreEqual(typeof(InvalidOperationException), asyncValidator.Exception.GetType());
        }

        private static async Task<string> ValidateFaultedAsync(DataRow dataRow)
        {
            await Task.Delay(200);
            throw new InvalidOperationException("Validation failed.");
        }

        [TestMethod]
        public void InputManager_AsyncValidators_Reset()
        {
            var dataSet = DataSet<ProductCategory>.Create();
            var _ = dataSet._;
            dataSet.Add(new DataRow());

            RowBinding<TextBox> textBox = null;
            var inputManager = dataSet.CreateInputManager(builder =>
            {
                textBox = _.Name.BindToTextBox();

                builder.GridColumns("100").GridRows("100")
                    .AddBinding(0, 0, textBox).WithRowValidationMode(ValidationMode.Implicit);

                builder.AddAsyncValidator(textBox.Input, dataRow => ValidateFaultedAsync(dataRow));
            });

            var currentRow = inputManager.CurrentRow;
            var asyncValidator = inputManager.RowValidation.AsyncValidators[0];

            textBox[currentRow].Text = "Anything";
            Assert.AreEqual(AsyncValidatorStatus.Running, asyncValidator.Status);

            asyncValidator.Reset();
            inputManager.InvalidateView();
            Assert.AreEqual(AsyncValidatorStatus.Inactive, asyncValidator.Status);
            Assert.IsNull(asyncValidator.Exception);
        }

        [TestMethod]
        public void InputManager_SetAsyncErrors()
        {
            var dataSet = DataSet<ProductCategory>.Create();
            var _ = dataSet._;
            dataSet.Add(new DataRow());

            RowBinding<TextBox> textBox = null;
            var inputManager = dataSet.CreateInputManager(builder =>
            {
                textBox = _.Name.BindToTextBox();
                builder.GridColumns("100").GridRows("100").AddBinding(0, 0, textBox).WithRowValidationMode(ValidationMode.Implicit);
            });

            var currentRow = inputManager.CurrentRow;
            Assert.IsNull(_.Name[0]);
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[currentRow]);
                Assert.AreEqual(1, errors.Count);
                var errorMessage = (DataValidationError)errors[0].ErrorContent;
                Assert.AreEqual(_.Name, errorMessage.Source);
            }

            var error = new DataValidationError("Result Error", _.Name);
            var results = DataValidationResults.Empty.Add(new DataValidationResult(currentRow.DataRow, error));
            inputManager.RowValidation.SetAsyncErrors(results);
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[currentRow]);
                Assert.AreEqual(2, errors.Count);
                Assert.AreEqual(error, errors[1].ErrorContent);
            }

            textBox[currentRow].Text = "any value";
            {
                var errors = System.Windows.Controls.Validation.GetErrors(textBox[currentRow]);
                Assert.AreEqual(0, errors.Count);
            }
        }

        // http://stackoverflow.com/questions/14087257/how-to-add-synchronization-context-to-async-test-method
        private static void RunInWpfSyncContext(Func<Task> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            var prevCtx = SynchronizationContext.Current;
            try
            {
                var syncCtx = new DispatcherSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(syncCtx);

                var task = func();
                if (task == null)
                    throw new InvalidOperationException();

                var frame = new DispatcherFrame();
                var t2 = task.ContinueWith(x => { frame.Continue = false; }, TaskScheduler.Default);
                Dispatcher.PushFrame(frame);   // execute all tasks until frame.Continue == false

                task.GetAwaiter().GetResult(); // rethrow exception when task has failed 
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(prevCtx);
            }
        }

    }
}
