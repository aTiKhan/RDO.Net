﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Provides service related utility methods.
    /// </summary>
    public static class ServiceManager
    {
        private static Dictionary<Type, Func<IService>> _serviceProviders = new Dictionary<Type, Func<IService>>();
        private static ConditionalWeakTable<DataPresenter, ConcurrentDictionary<Type, IService>> _services = new ConditionalWeakTable<DataPresenter, ConcurrentDictionary<Type, IService>>();

        /// <summary>
        /// Registers service by type.
        /// </summary>
        /// <typeparam name="TService">Type of service.</typeparam>
        /// <typeparam name="TServiceImpl">Type of service implementation.</typeparam>
        public static void Register<TService, TServiceImpl>()
            where TService : class, IService
            where TServiceImpl : TService, new()
        {
            var serviceType = typeof(TService);
            _serviceProviders.Add(serviceType, () => new TServiceImpl());
        }

        /// <summary>
        /// Registers service by type.
        /// </summary>
        /// <typeparam name="TService">Type of service.</typeparam>
        /// <typeparam name="TServiceImpl">Type of service implementation.</typeparam>
        /// <param name="createService">Delegate to create service implementation.</param>
        public static void Register<TService, TServiceImpl>(Func<TServiceImpl> createService)
            where TService : class, IService
            where TServiceImpl : TService
        {
            if (createService == null)
                throw new ArgumentNullException(nameof(createService));

            var serviceType = typeof(TService);
            _serviceProviders.Add(serviceType, () => createService());
        }

        /// <summary>
        /// Determines whether service is registered.
        /// </summary>
        /// <typeparam name="TService">Type of service.</typeparam>
        /// <returns><see langword="true"/> if service is registered, otherwise <see langword="false"/>.</returns>
        public static bool IsRegistered<TService>()
            where TService : class, IService
        {
            return _serviceProviders.ContainsKey(typeof(TService));
        }

        /// <summary>
        /// Gets the registered service for specified data presenter.
        /// </summary>
        /// <typeparam name="T">Type of service.</typeparam>
        /// <param name="dataPresenter">The data presenter.</param>
        /// <param name="autoCreate">Indicates whether service implementation should be created automatically.</param>
        /// <returns>The service.</returns>
        public static T GetRegisteredService<T>(this DataPresenter dataPresenter, bool autoCreate = true)
            where T : class, IService
        {
            if (dataPresenter == null)
                throw new ArgumentNullException(nameof(dataPresenter));

            if (autoCreate)
            {
                var servicesByType = _services.GetOrCreateValue(dataPresenter);
                return (T)servicesByType.GetOrAdd(typeof(T), type => CreateService<T>(dataPresenter));
            }
            else if (_services.TryGetValue(dataPresenter, out var servicesByType))
            {
                if (servicesByType.ContainsKey(typeof(T)))
                    return (T)servicesByType[typeof(T)];
            }

            return null;
        }

        internal static void Reset(DataPresenter dataPresenter, bool isReload)
        {
            Debug.Assert(dataPresenter != null);

            if (!_services.TryGetValue(dataPresenter, out var servicesByType))
                return;

            ConcurrentDictionary<Type, IService> reloadableServicesByType = null;
            if (isReload)
            {
                foreach (var keyValuePair in servicesByType)
                {
                    var service = keyValuePair.Value;
                    if (service is IReloadableService)
                    {
                        var type = keyValuePair.Key;
                        if (reloadableServicesByType == null)
                            reloadableServicesByType = new ConcurrentDictionary<Type, IService>();
                        reloadableServicesByType.TryAdd(type, service);
                    }
                }
            }

            _services.Remove(dataPresenter);
            if (reloadableServicesByType != null)
                _services.Add(dataPresenter, reloadableServicesByType);
        }

        private static T CreateService<T>(DataPresenter dataPresenter)
            where T : class, IService
        {
            if (_serviceProviders.TryGetValue(typeof(T), out var constructor))
            {
                var result = (T)constructor();
                result.Initialize(dataPresenter);
                return result;
            }
            return null;
        }

        private sealed class ServiceIdentifier
        {
            public ServiceIdentifier(IService service, Type type)
            {
                Debug.Assert(service != null);
                Debug.Assert(type != null);
                Service = service;
                Type = type;
            }

            public readonly IService Service;
            public readonly Type Type;
        }

        private sealed class ServiceIdentifierBag : KeyedCollection<Type, ServiceIdentifier>
        {
            protected override Type GetKeyForItem(ServiceIdentifier item)
            {
                return item.Type;
            }
        }

        private static ConditionalWeakTable<object, ServiceIdentifierBag> s_serviceIdentifiers = new ConditionalWeakTable<object, ServiceIdentifierBag>();

        private static ServiceIdentifier GetServiceIdentifier(this IService service, Type type)
        {
            Debug.Assert(service != null);
            Debug.Assert(type != null);

            ServiceIdentifierBag bag = s_serviceIdentifiers.GetOrCreateValue(service);
            if (bag.Contains(type))
                return bag[type];
            else
            {
                var result = new ServiceIdentifier(service, type);
                bag.Add(result);
                return result;
            }
        }

        private static bool Setup(this UIElement element, ServiceIdentifier serviceIdentifier)
        {
            var bag = s_serviceIdentifiers.GetOrCreateValue(element);
            if (bag.Contains(serviceIdentifier.Type))
                return false;

            bag.Add(serviceIdentifier);
            return true;
        }

        private static bool Cleanup(this UIElement element)
        {
            return s_serviceIdentifiers.Remove(element);
        }

        private sealed class CommandBindingEx : CommandBinding
        {
            public CommandBindingEx(ICommand command, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute)
                : base(command, executed, canExecute)
            {
            }
        }

        private sealed class InputBindingEx : InputBinding
        {
            public InputBindingEx(ICommand command, InputGesture inputGesture)
                : base(command, inputGesture)
            {
            }
        }

        /// <summary>
        /// Setup command entries for specified view element.
        /// </summary>
        /// <typeparam name="TService">Type of service.</typeparam>
        /// <typeparam name="TElement">Type of view element.</typeparam>
        /// <param name="element">The view element.</param>
        /// <param name="commandService"></param>
        /// <param name="getCommandEntries">Delegate to get command entries.</param>
        public static void SetupCommandEntries<TService, TElement>(this TElement element, TService commandService,  Func<TService, TElement, IEnumerable<CommandEntry>> getCommandEntries)
            where TService : IService
            where TElement : UIElement
        {
            element.VerifyNotNull(nameof(element));
            if (commandService == null)
                throw new ArgumentNullException(nameof(commandService));
            getCommandEntries.VerifyNotNull(nameof(getCommandEntries));

            var serviceIdentifier = commandService.GetServiceIdentifier(typeof(TService));
            if (!element.Setup(serviceIdentifier))
                return;

            var commandEntries = getCommandEntries(commandService, element);
            if (commandEntries == null)
                return;
            foreach (var entry in commandEntries)
            {
                if (entry.Executed != null)
                {
                    var commandBinding = new CommandBindingEx(entry.Command, entry.Executed, entry.CanExecute);
                    element.CommandBindings.Add(commandBinding);
                }
                for (int i = 0; i < entry.InputGesturesCount; i++)
                {
                    var inputBinding = new InputBindingEx(entry.Command, entry.GetInputGesture(i));
                    element.InputBindings.Add(inputBinding);
                }
            }
        }

        /// <summary>
        /// Cleanup command entries for specified view element.
        /// </summary>
        /// <param name="element">The view element.</param>
        public static void CleanupCommandEntries(this UIElement element)
        {
            element.VerifyNotNull(nameof(element));

            if (!element.Cleanup())
                return;

            CleanupCommandBindings(element.CommandBindings);
            CleanupInputBindings(element.InputBindings);
        }

        private static void CleanupCommandBindings(CommandBindingCollection commandBindings)
        {
            for (int i = commandBindings.Count - 1; i >= 0; i--)
            {
                if (commandBindings[i] is CommandBindingEx)
                    commandBindings.RemoveAt(i);
            }
        }

        private static void CleanupInputBindings(InputBindingCollection inputBindings)
        {
            for (int i = inputBindings.Count - 1; i >= 0; i--)
            {
                if (inputBindings[i] is InputBindingEx)
                    inputBindings.RemoveAt(i);
            }
        }

        /// <summary>
        /// Binds command with command implementation.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="executed">The command Executed handler.</param>
        /// <param name="canExecute">The command CanExecute handler.</param>
        /// <returns>The command entry contains both command and command implementation.</returns>
        public static CommandEntry Bind(this ICommand command, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute = null)
        {
            Verify(command, executed);
            return new CommandEntry(command, executed, canExecute, null);
        }

        /// <summary>
        /// Binds command with command implementation.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="executed">The command Executed handler.</param>
        /// <param name="inputGesture">The input gesture.</param>
        /// <returns>The command entry contains both command and command implementation.</returns>
        public static CommandEntry Bind(this ICommand command, ExecutedRoutedEventHandler executed, InputGesture inputGesture)
        {
            return Bind(command, executed, (CanExecuteRoutedEventHandler)null, inputGesture);
        }

        /// <summary>
        /// Binds command with command implementation.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="executed">The command Executed handler.</param>
        /// <param name="canExecute">The command CanExecute handler.</param>
        /// <param name="inputGesture">The input gesture.</param>
        /// <returns>The command entry contains both command and command implementation.</returns>
        public static CommandEntry Bind(this ICommand command, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute, InputGesture inputGesture)
        {
            Verify(command, executed, inputGesture);
            return new CommandEntry(command, executed, canExecute, inputGesture);
        }

        /// <summary>
        /// Binds command with command implementation.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="executed">The command Executed handler.</param>
        /// <param name="inputGestures">The input gestures.</param>
        /// <returns>The command entry contains both command and command implementation.</returns>
        public static CommandEntry Bind(this ICommand command, ExecutedRoutedEventHandler executed, params InputGesture[] inputGestures)
        {
            return Bind(command, executed, null, inputGestures);
        }

        /// <summary>
        /// Binds command with command implementation.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="executed">The command Executed handler.</param>
        /// <param name="canExecute">The command CanExecute handler.</param>
        /// <param name="inputGestures">The input gestures.</param>
        /// <returns>The command entry contains both command and command implementation.</returns>
        public static CommandEntry Bind(this ICommand command, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute, params InputGesture[] inputGestures)
        {
            Verify(command, executed, inputGestures);
            return new CommandEntry(command, executed, canExecute, inputGestures);
        }

        /// <summary>
        /// Binds command with command implementation.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="inputGestures">The input gestures.</param>
        /// <param name="executed">The command Executed handler.</param>
        /// <param name="canExecute">The command CanExecute handler.</param>
        /// <returns>The command entry contains both command and command implementation.</returns>
        public static CommandEntry Bind(this ICommand command, IReadOnlyList<InputGesture> inputGestures, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute)
        {
            Verify(command, executed, inputGestures);
            return new CommandEntry(command, executed, canExecute, inputGestures);
        }

        /// <summary>
        /// Binds command with command implementation.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="inputGesture">The input gesture.</param>
        /// <returns>The command entry contains both command and command implementation.</returns>
        public static CommandEntry Bind(this ICommand command, InputGesture inputGesture)
        {
            Verify(command);
            Verify(inputGesture);
            return new CommandEntry(command, null, null, inputGesture);
        }

        /// <summary>
        /// Binds command with command implementation.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="inputGestures">The input gestures.</param>
        /// <returns>The command entry contains both command and command implementation.</returns>
        public static CommandEntry Bind(this ICommand command, params InputGesture[] inputGestures)
        {
            Verify(command);
            Verify(inputGestures);
            return new CommandEntry(command, null, null, inputGestures);
        }

        private static void Verify(ICommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
        }

        private static void Verify(ICommand command, ExecutedRoutedEventHandler executed)
        {
            Verify(command);
            if (executed == null)
                throw new ArgumentNullException(nameof(executed));
        }

        private static void Verify(ICommand command, ExecutedRoutedEventHandler executed, InputGesture inputGesture)
        {
            Verify(command, executed);
            Verify(inputGesture);
        }

        private static void Verify(InputGesture inputGesture)
        {
            if (inputGesture == null)
                throw new ArgumentNullException(nameof(inputGesture));
        }

        private static void Verify(ICommand command, ExecutedRoutedEventHandler executed, IReadOnlyList<InputGesture> inputGestures)
        {
            Verify(command, executed);
            Verify(inputGestures);
        }

        private static void Verify(IReadOnlyList<InputGesture> inputGestures)
        {
            if (inputGestures == null || inputGestures.Count == 0)
                return;

            for (int i = 0; i < inputGestures.Count; i++)
            {
                if (inputGestures[i] == null)
                    throw new ArgumentException(DiagnosticMessages._ArgumentNullAtIndex(nameof(inputGestures), i), nameof(inputGestures));
            }
        }
    }
}
