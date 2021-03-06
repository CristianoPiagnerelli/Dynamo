﻿//Copyright © Autodesk, Inc. 2012. All rights reserved.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Expression = Dynamo.FScheme.Expression;

using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;

namespace Dynamo.FSchemeInterop
{
   //Class wrapping an FScheme environment.
   internal class EnvironmentWrapper
   {
      //Our FScheme environment.
      private FSharpRef<FSharpMap<string, FSharpRef<Expression>>> env;

      //Public property accessor.
      public FSharpRef<FSharpMap<string, FSharpRef<Expression>>> Env
      {
         get { return this.env; }
      }

      //Default constructor. Sets the environment contained by this EnvironmentWrapper
      //to the one provided by FScheme by default.
      public EnvironmentWrapper()
      {
         this.env = FScheme.environment;
      }

      public EnvironmentWrapper(FSharpRef<FSharpMap<string, FSharpRef<Expression>>> e)
      {
         this.env = e;
      }

      //Indexor providing a quick way to lookup symbols in the environment.
      public FSharpRef<Expression> this[string symbol]
      {
         get
         {
            return this.Lookup(symbol);
         }
      }

      //Looks up the given symbol in this environment.
      public FSharpRef<Expression> Lookup(string symbol)
      {
         return FScheme.lookup(env, symbol);
      }

      //Adds a symbol to this environment.
      public void Add(string symbol, Expression expr)
      {
         this.env.Value = MapModule.Add(symbol, new FSharpRef<Expression>(expr), this.env.contents);
      }

      public void Delete(string symbol)
      {
         this.env.Value = MapModule.Remove(symbol, this.env.contents);
      }
   }

   //Class representing an FScheme Execution Environment. Used to evaluate FScheme Expressions.
   public class ExecutionEnvironment
   {
      //Environment used to store symbols.
      private EnvironmentWrapper env;

      //Identity function, used as the default FScheme Continuation.
      public static FSharpFunc<Expression, Expression> IDENT
         = FuncConvert.ToFSharpFunc((Converter<Expression, Expression>)(x => x));

      //Default constructor, simply creates a default environment.
      public ExecutionEnvironment()
      {
         this.env = new EnvironmentWrapper();
      }

      public ExecutionEnvironment(FSharpRef<FSharpMap<string, FSharpRef<Expression>>> e)
      {
         this.env = new EnvironmentWrapper(e);
      }

      //Binds symbols of the given string to the given body.
      public void DefineSymbol(string name, Expression body)
      {
         FScheme.Define(
            IDENT, env.Env, Utils.mkList(Expression.NewSymbol(name), body)
         );
         //this.env.Add(name, body);
      }

      //Binds symbols of the given string to the given Expression.
      //private void DefineExternal(string name, Expression func)
      //{
      //   this.env.Add(name, func);
      //}

      //Binds symbols of the given string to the given External Function.
      public void DefineExternal(string name, FScheme.ExternFunc func)
      {
         this.DefineSymbol(name, FuncContainer.MakeFunction(func));
      }

      //Evaluates the given expression.
      public Expression Evaluate(Expression expr)
      {
         return FScheme.eval(IDENT, env.Env, expr);
      }

      ///Removes the given symbol from the environment.
      public void RemoveSymbol(string p)
      {
         this.env.Delete(p);
      }

      public Expression LookupSymbol(string p)
      {
         return this.env.Env.Value[p].Value;
      }

      //Public property accessor.
      public FSharpRef<FSharpMap<string, FSharpRef<Expression>>> Env
      {
         get { return this.env.Env; }
      }
   }
}
