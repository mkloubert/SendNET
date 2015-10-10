/**********************************************************************************************************************
 * Send.NET (https://github.com/mkloubert/SendNET)                                                                    *
 *                                                                                                                    *
 * Copyright (c) 2015, Marcel Joachim Kloubert <marcel.kloubert@gmx.net>                                              *
 * All rights reserved.                                                                                               *
 *                                                                                                                    *
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the   *
 * following conditions are met:                                                                                      *
 *                                                                                                                    *
 * 1. Redistributions of source code must retain the above copyright notice, this list of conditions and the          *
 *    following disclaimer.                                                                                           *
 *                                                                                                                    *
 * 2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the       *
 *    following disclaimer in the documentation and/or other materials provided with the distribution.                *
 *                                                                                                                    *
 * 3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote    *
 *    products derived from this software without specific prior written permission.                                  *
 *                                                                                                                    *
 *                                                                                                                    *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, *
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE  *
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, *
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR    *
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,  *
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE   *
 * USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.                                           *
 *                                                                                                                    *
 **********************************************************************************************************************/

using MarcelJoachimKloubert.SendNET.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MarcelJoachimKloubert.SendNET.ComponentModel
{
    /// <summary>
    /// A basic notification object.
    /// </summary>
    public abstract partial class NotifiableBase : MarshalByRefObject, INotifyPropertyChanged
    {
        #region Fields (2)

        /// <summary>
        /// Stores the object for thread safe operations.
        /// </summary>
        protected readonly object _SYNC;

        private IDictionary<string, object> _PROPERTIES;

        #endregion Fields (2)

        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifiableBase" /> class.
        /// </summary>
        /// <param name="sync">The value for the <see cref="NotifiableBase.SyncRoot" /> property.</param>
        protected NotifiableBase(object sync = null)
        {
            this._SYNC = sync ?? new object();

            this._PROPERTIES = this.CreatePropertyStorage() ?? new Dictionary<string, object>();
        }

        #endregion Constructors (1)

        #region Events (2)

        /// <summary>
        /// Is raised on an error.
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;

        /// <summary>
        /// <see cref="INotifyPropertyChanged.PropertyChanged" />
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events (2)

        #region Properties (1)

        /// <summary>
        /// <see cref="NotifiableBase._SYNC" />
        /// </summary>
        public object SyncRoot
        {
            get { return this._SYNC; }
        }

        #endregion Properties (1)

        #region Methods (24)

        /// <summary>
        /// Creates property storage.
        /// </summary>
        /// <returns>The created dictionary.</returns>
        protected virtual IDictionary<string, object> CreatePropertyStorage()
        {
            return null;
        }

        /// <summary>
        /// Converts a property value to a target type.
        /// </summary>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="obj">The input value.</param>
        /// <returns>The output value.</returns>
        protected virtual TTarget ConvertPropertyValue<TTarget>(string propertyName, object obj)
        {
            return this.ConvertTo<TTarget>(obj);
        }

        /// <summary>
        /// Converts an object to a target type.
        /// </summary>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="obj">The input value.</param>
        /// <returns>The output value.</returns>
        protected virtual TTarget ConvertTo<TTarget>(object obj)
        {
            return (TTarget)obj;
        }

        /// <summary>
        /// Returns the value of a property.
        /// </summary>
        /// <typeparam name="TProperty">Type of the property.</typeparam>
        /// <param name="expr">The expression that contains the name of the property.</param>
        /// <returns>The value.</returns>
        /// <exception cref="ArgumentException">
        /// Body of <paramref name="expr" /> is no <see cref="MemberExpression" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Body of <paramref name="expr" /> is no <see cref="MemberExpression" />.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// Member expression does not contain a <see cref="_PropertyInfo" />.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// Property was not found.
        /// </exception>
        protected TProperty Get<TProperty>(Expression<Func<TProperty>> expr)
        {
            bool found;
            return this.Get(expr, out found);
        }

        /// <summary>
        /// Returns the value of a property.
        /// </summary>
        /// <typeparam name="TProperty">Type of the property.</typeparam>
        /// <param name="expr">The expression that contains the name of the property.</param>
        /// <param name="found">Stores if value exists / was found or not.</param>
        /// <returns>The value.</returns>
        /// <exception cref="ArgumentException">
        /// Body of <paramref name="expr" /> is no <see cref="MemberExpression" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Body of <paramref name="expr" /> is no <see cref="MemberExpression" />.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// Member expression does not contain a <see cref="_PropertyInfo" />.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// Property was not found.
        /// </exception>
        protected TProperty Get<TProperty>(Expression<Func<TProperty>> expr, out bool found)
        {
            return this.Get<TProperty>(propertyName: GetPropertyName(expr),
                                       found: out found);
        }

        /// <summary>
        /// Returns the value of a property.
        /// </summary>
        /// <typeparam name="TProperty">Type of the property.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The value.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="propertyName" /> is invalid.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="propertyName" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// Property was not found.
        /// </exception>
        protected TProperty Get<TProperty>(IEnumerable<char> propertyName)
        {
            bool found;
            return this.Get<TProperty>(propertyName, out found);
        }

        /// <summary>
        /// Returns the value of a property.
        /// </summary>
        /// <typeparam name="TProperty">Type of the property.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="found">Stores if value exists / was found or not.</param>
        /// <returns>The value.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="propertyName" /> is invalid.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="propertyName" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// Property was not found.
        /// </exception>
        protected TProperty Get<TProperty>(IEnumerable<char> propertyName, out bool found)
        {
            var pn = this.NormalizePropertyName(propertyName);

            object temp;
            if (this._PROPERTIES.TryGetValue(pn, out temp))
            {
                found = true;
                return this.ConvertPropertyValue<TProperty>(pn, temp);
            }

            found = false;
            return this.GetDefaultPropertyValue<TProperty>(pn);
        }

        /// <summary>
        /// Returns the default value for a type.
        /// </summary>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <returns>The default value.</returns>
        protected virtual TValue GetDefaultValue<TValue>()
        {
            return default(TValue);
        }

        /// <summary>
        /// Returns the default value for a property.
        /// </summary>
        /// <typeparam name="TProperty">Property type.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The default value.</returns>
        protected virtual TProperty GetDefaultPropertyValue<TProperty>(string propertyName)
        {
            return this.GetDefaultValue<TProperty>();
        }

        /// <summary>
        /// Returns the name of a property from an expression.
        /// </summary>
        /// <typeparam name="TProperty">Type of the property.</typeparam>
        /// <param name="expr">The expression.</param>
        /// <returns>The name of the property.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="expr" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Body of <paramref name="expr" /> is no <see cref="MemberExpression" />.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// Member expression does not contain a <see cref="_PropertyInfo" />.
        /// </exception>
        protected static string GetPropertyName<TProperty>(Expression<Func<TProperty>> expr)
        {
            if (expr == null)
            {
                throw new ArgumentNullException("expr");
            }

            var memberExpr = expr.Body as MemberExpression;
            if (memberExpr == null)
            {
                throw new ArgumentException("expr.Body");
            }

            return ((_PropertyInfo)memberExpr.Member).Name;
        }

        /// <summary>
        /// Returns the <see cref="IEqualityComparer{T}" /> for a property.
        /// </summary>
        /// <typeparam name="TProperty">Type of the property.</typeparam>
        /// <returns>The instance.</returns>
        protected virtual IEqualityComparer<TProperty> GetPropertyValueEqualityComparer<TProperty>(string propertyName)
        {
            return null;
        }

        private void HandleReceiveNotificationFromAttributes(string propertyName)
        {
            foreach (var property in this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                foreach (var attrib in property.GetCustomAttributes(typeof(ReceiveNotificationFromAttribute), false)
                                               .Cast<ReceiveNotificationFromAttribute>())
                {
                    if ((attrib.SenderName ?? string.Empty).Trim() != propertyName)
                    {
                        // does not match
                        continue;
                    }

                    if (property.Name == propertyName)
                    {
                        // not allowed
                        continue;
                    }

                    // does match => raise
                    this.RaisePropertyChanged(property.Name);
                }
            }
        }

        private void HandleReceiveValueFromAttributes<T>(string propertyName, T oldValue, T newValue)
        {
            var members = Enumerable.Empty<_MemberInfo>()
                                    .Concat(this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                                    .Concat(this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                                    .Concat(this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));

            foreach (var m in members)
            {
                foreach (var attrib in m.GetCustomAttributes(typeof(ReceiveValueFromAttribute), false)
                                        .Cast<ReceiveValueFromAttribute>())
                {
                    if ((attrib.SenderName ?? string.Empty).Trim() != propertyName)
                    {
                        // does not match
                        continue;
                    }

                    var args = new ReceiveValueFromArgs(this)
                    {
                        NewValue = newValue,
                        OldValue = oldValue,
                        SenderName = propertyName,
                        SenderType = MemberTypes.Property,
                    };

                    if (m is _MethodInfo)
                    {
                        var method = (_MethodInfo)m;

                        var @params = method.GetParameters();
                        if (@params.Length < 1)
                        {
                            method.Invoke(this,
                                          new object[] { });
                        }
                        else if (@params.Length < 2)
                        {
                            method.Invoke(this,
                                          new object[] { args });
                        }
                    }
                    else if (m is _PropertyInfo)
                    {
                        var property = (_PropertyInfo)m;

                        if (property.Name == propertyName)
                        {
                            // not allowed
                            continue;
                        }

                        if (!property.CanWrite)
                        {
                            // no setter
                            continue;
                        }

                        property.SetValue(this, args.NewValue, null);
                    }
                    else if (m is _FieldInfo)
                    {
                        var field = (_FieldInfo)m;

                        field.SetValue(this, args.NewValue);
                    }
                }
            }
        }

        /// <summary>
        /// Normalizes a property name.
        /// </summary>
        /// <param name="propertyName">The input value.</param>
        /// <returns>The output value.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="propertyName" /> is invalid.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="propertyName" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// Property was not found.
        /// </exception>
        protected string NormalizePropertyName(IEnumerable<char> propertyName)
        {
            var result = propertyName.AsString();

            if (result == null)
            {
                throw new ArgumentNullException("pn");
            }

            result = result.Trim();

            if (result == string.Empty)
            {
                throw new ArgumentException("pn");
            }

#if DEBUG
            if (global::System.ComponentModel.TypeDescriptor.GetProperties(this)[result] == null)
            {
                throw new global::System.MissingMemberException(className: this.GetType().FullName,
                                                                memberName: result);
            }
#endif

            return result;
        }

        /// <summary>
        /// Raises the <see cref="NotifiableBase.Error" /> event.
        /// </summary>
        /// <param name="ex">The underlying exception.</param>
        /// <param name="rethrow">Rethrow exception or not.</param>
        /// <returns>
        /// Handler was raised (<see langword="true" />) or not (<see langword="false" />).
        /// <see langword="null" /> indicates that <paramref name="ex" /> is <see langword="null" />.
        /// </returns>
        /// <exception cref="ApplicationException">The (wrapped) exception of <paramref name="ex" />.</exception>
        protected bool? RaiseError(Exception ex, bool rethrow = false)
        {
            if (ex == null)
            {
                return null;
            }

            var e = new ErrorEventArgs(ex);
            var result = this.RaiseEventHandler(this.Error, e);

            if (rethrow)
            {
                throw e.GetException();
            }

            return result;
        }

        /// <summary>
        /// Raises the <see cref="NotifiableBase.Error" /> event.
        /// </summary>
        /// <param name="ex">The underlying exception.</param>
        /// <param name="code">The rror code.</param>
        /// <param name="rethrow">Rethrow exception or not.</param>
        /// <returns>
        /// Handler was raised (<see langword="true" />) or not (<see langword="false" />).
        /// <see langword="null" /> indicates that <paramref name="ex" /> is <see langword="null" />.
        /// </returns>
        protected bool? RaiseError(Exception ex, int code, bool rethrow = false)
        {
            if (ex == null)
            {
                return null;
            }

            return this.RaiseError(new ApplicationException(code: code,
                                                            message: ex != null ? ex.Message : null,
                                                            innerException: ex),
                                   rethrow);
        }

        /// <summary>
        /// Raises an event handler.
        /// </summary>
        /// <param name="handler">The handler to raise.</param>
        /// <returns>Handler was raised (<see langword="true" />); otherwise <paramref name="handler" /> is <see langword="null" />.</returns>
        protected bool RaiseEventHandler(EventHandler handler)
        {
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Raises an event handler.
        /// </summary>
        /// <typeparam name="TArgs">Type of the event arguments.</typeparam>
        /// <param name="handler">The handler to raise.</param>
        /// <param name="e">The arguments for the event.</param>
        /// <returns>Handler was raised (<see langword="true" />); otherwise <paramref name="handler" /> is <see langword="null" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="e" /> is <see langword="null" />.
        /// </exception>
        protected bool RaiseEventHandler<TArgs>(EventHandler<TArgs> handler, TArgs e)
            where TArgs : global::System.EventArgs
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            if (handler != null)
            {
                handler(this, e);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Raises the <see cref="INotifyPropertyChanged.PropertyChanged" /> event.
        /// </summary>
        /// <param name="expr">The expression that contains the property name.</param>
        /// <returns>Handler was raised or not.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="expr" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Body of <paramref name="expr" /> is no <see cref="MemberExpression" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Body of <paramref name="expr" /> is no <see cref="MemberExpression" />.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// Member expression does not contain a <see cref="_PropertyInfo" />.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// Property was not found.
        /// </exception>
        protected bool RaisePropertyChanged<T>(Expression<Func<T>> expr)
        {
            return this.RaisePropertyChanged(propertyName: GetPropertyName(expr));
        }

        /// <summary>
        /// Raises the <see cref="INotifyPropertyChanged.PropertyChanged" /> event.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>Handler was raised or not.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="propertyName" /> is invalid.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="propertyName" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// Property was not found.
        /// </exception>
        protected bool RaisePropertyChanged(IEnumerable<char> propertyName)
        {
            var pn = this.NormalizePropertyName(propertyName);

            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(pn));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets a property.
        /// </summary>
        /// <typeparam name="TProperty">Type of the property.</typeparam>
        /// <param name="expr">The expression that contains the name of the property.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>
        /// <paramref name="newValue" /> is different to current value (<see langword="true" />); otherwise <see langword="false" />.
        /// <see langword="null" /> indicates that old and new value are different, but that
        /// <see cref="NotifiableBase.PropertyChanged" /> event was NOT raised.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="expr" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Body of <paramref name="expr" /> is no <see cref="MemberExpression" />.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// Member expression does not contain a <see cref="_PropertyInfo" />.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// Property was not found.
        /// </exception>
        protected bool? Set<TProperty>(Expression<Func<TProperty>> expr, TProperty newValue)
        {
            return this.Set<TProperty>(propertyName: GetPropertyName(expr),
                                       newValue: newValue);
        }

        /// <summary>
        /// Sets a property.
        /// </summary>
        /// <typeparam name="TProperty">Type of the property.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>
        /// <paramref name="newValue" /> is different to current value (<see langword="true" />); otherwise <see langword="false" />.
        /// <see langword="null" /> indicates that old and new value are different, but that
        /// <see cref="NotifiableBase.PropertyChanged" /> event was NOT raised.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="propertyName" /> is invalid.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="propertyName" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// Property was not found.
        /// </exception>
        protected bool? Set<TProperty>(IEnumerable<char> propertyName, TProperty newValue)
        {
            var pn = this.NormalizePropertyName(propertyName);

            TProperty oldValue = this.Get<TProperty>(pn);

            var comparer = this.GetPropertyValueEqualityComparer<TProperty>(pn) ?? ObjectComparer<TProperty>.Default;
            if (!comparer.Equals(oldValue, newValue))
            {
                this._PROPERTIES.AddOrSet(pn, newValue);

                this.HandleReceiveNotificationFromAttributes(pn);
                this.HandleReceiveValueFromAttributes(pn, oldValue, newValue);

                return this.RaisePropertyChanged(pn) ? (bool?)true : null;
            }

            return false;
        }

        #endregion Methods (24)
    }
}