/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Reflection.Emit;
using System.Collections.ObjectModel;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {

    public class IfStatement : Statement {
        private readonly ReadOnlyCollection<IfStatementTest> _tests;
        private Statement _else;

        internal IfStatement(SourceSpan span, ReadOnlyCollection<IfStatementTest> /*!*/ tests, Statement @else)
            : base(AstNodeType.IfStatement, span) {
            _tests = tests;
            _else = @else;
        }

        public ReadOnlyCollection<IfStatementTest> Tests {
            get { return _tests; }
        }

        public Statement ElseStatement {
            get { return _else; }
          set { _else = value; }
        }

        public override void Emit(CodeGen cg) {
            bool eoiused = false;
            Label eoi = cg.DefineLabel();
            foreach (IfStatementTest t in _tests) {
                Label next = cg.DefineLabel();

                if (t.Test.Span.IsValid)
                {
                  cg.EmitPosition(t.Test.Start, t.Test.End);
                }
                
                t.Test.EmitBranchFalse(cg, next);

                t.Body.Emit(cg);

                // optimize no else case
                if (IsNotIfOrReturn(t.Body))
                {
                  eoiused = true;
                  cg.Emit(OpCodes.Br, eoi);
                }
                cg.MarkLabel(next);
            }
            if (_else != null) {
                _else.Emit(cg);
            }
            if (eoiused)
            {
              cg.MarkLabel(eoi);
            }
        }

        static bool IsNotIfOrReturn(Statement s)
        {
          if (s is BlockStatement)
          {
            BlockStatement bs = (BlockStatement)s;
            return IsNotIfOrReturn(bs.Statements[bs.Statements.Count - 1]);
          }
          else if (s is IfStatement)
          {
            var ifs = s as IfStatement;
            return IsNotIfOrReturn(ifs.Tests[0].Body) || (ifs.ElseStatement != null && IsNotIfOrReturn(ifs.ElseStatement));
            
          }
          else
          {
            if (!(s is ReturnStatement) && !(s is ContinueStatement))
            {
              return true;
            }
          }
          return false;
        }
    }
}
