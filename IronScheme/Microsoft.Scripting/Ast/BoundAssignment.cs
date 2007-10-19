/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Diagnostics;
using System.Reflection.Emit;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Actions;
using System.Reflection;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public class BoundAssignment : Expression {
        private readonly Variable _variable;
        private readonly Expression _value;
        private bool _defined;

        // implementation detail.
        private VariableReference _vr;

        internal BoundAssignment(Variable variable, Expression value) {
            Contract.RequiresNotNull(variable, "variable");
            Contract.RequiresNotNull(value, "value");
            _variable = variable;
            _value = value;
        }

        public Variable Variable {
            get { return _variable; }
        }

        internal VariableReference Ref {
            get { return _vr; }
            set {
                Debug.Assert(value != null);
                Debug.Assert(value.Variable == _variable);
                Debug.Assert(_vr == null || _vr.Equals(value));
                _vr = value;
            }
        }

        public Expression Value {
            get { return _value; }
        }

        internal bool IsDefined {
            get { return _defined; }
            set { _defined = value; }
        }

        public override Type Type {
            get {
                return _variable.Type;
            }
        }

        internal override void EmitAddress(CodeGen cg, Type asType) {
            _value.Emit(cg);
            _vr.Slot.EmitSet(cg);
            _vr.Slot.EmitGetAddr(cg);
        }

        public override void Emit(CodeGen cg) {
            _value.Emit(cg);
            cg.Emit(OpCodes.Dup);
            _vr.Slot.EmitSet(cg);
        }

        protected override object DoEvaluate(CodeContext context) {
            object value = _value.Evaluate(context);

            // Do an explicit conversion to mirror the emit case
            value = context.LanguageContext.Binder.Convert(value, _variable.Type);

            EvaluateAssign(context, _variable, value);
            return value;
        }

        internal override object EvaluateAssign(CodeContext context, object value) {
            return EvaluateAssign(context, Variable, value);
        }

        internal static object EvaluateAssign(CodeContext context, Variable var, object value) {
            switch (var.Kind) {
                case Variable.VariableKind.Temporary:
                case Variable.VariableKind.GeneratorTemporary:
                    context.Scope.TemporaryStorage[var] = value;
                    break;
                case Variable.VariableKind.Global:
                    RuntimeHelpers.SetGlobalName(context, var.Name, value);
                    break;
                default:
                    RuntimeHelpers.SetName(context, var.Name, value);
                    break;
            }
            return value;
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _value.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }

    public static partial class Ast {
        /// <summary>
        /// Performs an assignment variable = value
        /// </summary>
        public static Statement Write(Variable variable, Variable value) {
            return Statement(Assign(variable, Ast.Read(value)));
        }

        /// <summary>
        /// Performs an assignment variable = value
        /// </summary>
        public static Statement Write(Variable variable, Expression value) {
            return Statement(Assign(variable, value));
        }

        /// <summary>
        /// Performs an assignment variable.field = value
        /// </summary>
        public static Statement Write(Variable variable, FieldInfo field, Expression value) {
            return Statement(AssignField(Read(variable), field, value));
        }

        /// <summary>
        /// Performs an assignment variable.field = value
        /// </summary>
        public static Statement Write(Variable variable, FieldInfo field, Variable value) {
            return Statement(AssignField(Read(variable), field, Read(value)));
        }

        /// <summary>
        /// Performs an assignment variable = right.field
        /// </summary>
        public static Statement Write(Variable variable, Variable right, FieldInfo field) {
            return Statement(Assign(variable, ReadField(Read(right), field)));
        }

        /// <summary>
        /// Performs an assignment variable.leftField = right.rightField
        /// </summary>
        public static Statement Write(Variable variable, FieldInfo leftField, Variable right, FieldInfo rightField) {
            return Statement(AssignField(Read(variable), leftField, ReadField(Read(right), rightField)));
        }

        /// <summary>
        /// Performs an assignment variable = value
        /// </summary>
        public static BoundAssignment Assign(Variable variable, Expression value) {
            Contract.RequiresNotNull(variable, "variable");
            Contract.RequiresNotNull(value, "value");
            Contract.Requires(TypeUtils.CanAssign(variable.Type, value.Type));
            return new BoundAssignment(variable, value);
        }
    }
}