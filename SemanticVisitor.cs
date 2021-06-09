using System;
using System.Collections.Generic;

namespace drac {

		class FunctionProperties{
        	public Boolean isPrimitive;
			public int ariety ;
			public HashSet<string> simbolicTable;
			

			public FunctionProperties(Boolean isPrimitive, int ariety, HashSet<string> simbolicTable){
				this.isPrimitive = isPrimitive;
				this.ariety = ariety;
				this.simbolicTable = simbolicTable;
			}

			public override string ToString(){
				var strFunctionProp =  "";

				strFunctionProp += " isPrimitive: " + this.isPrimitive.ToString() + " ";
				strFunctionProp += " ariety: " + this.ariety.ToString() + " ";
				strFunctionProp += " vars: ";

				if(simbolicTable?.Count > 0){
					foreach(string varName in this.simbolicTable){
						strFunctionProp += varName + ", ";
					}
				}

				return strFunctionProp;
			}

    	}

		class SemanticVisitor {

				//-----------------------------------------------------------
				public HashSet<string> TableGlobalVar {
						get;
						private set;
				}
				public IDictionary<string, FunctionProperties> TableFunc {
						get;
						private set;
				}
				//-----------------------------------------------------------

				int noOfPass = 1;
				int lvlLoop = 0;
				string currentFuncName = "";

				public SemanticVisitor() {
						TableGlobalVar = new HashSet<string>();
						TableFunc = new SortedDictionary<string, FunctionProperties>();

						TableFunc.Add("printi", new FunctionProperties(true, 1, null));
						TableFunc.Add("printc", new FunctionProperties(true, 1, null));
						TableFunc.Add("prints", new FunctionProperties(true, 1, null));
						TableFunc.Add("println", new FunctionProperties(true, 0, null));
						TableFunc.Add("readi", new FunctionProperties(true, 0, null));
						TableFunc.Add("reads", new FunctionProperties(true, 0, null));
						TableFunc.Add("new", new FunctionProperties(true, 1, null));
						TableFunc.Add("size", new FunctionProperties(true, 1, null));
						TableFunc.Add("add", new FunctionProperties(true, 2, null));
						TableFunc.Add("get", new FunctionProperties(true, 2, null));
						TableFunc.Add("set", new FunctionProperties(true, 3, null));
						
				}

				public void Visit(DefList node){
						VisitChildren((dynamic) node);

						if(!TableFunc.ContainsKey("main")){
							throw new SemanticError("No main function in the program.");
						}

						noOfPass += 1;

						VisitChildren((dynamic) node);
				}

				public void Visit(VarDef node){
						if(noOfPass == 1){
							foreach(var n in node[0]){
								if(n is Identifier){
									var varname = n.AnchorToken.Lexeme;
									if (TableGlobalVar.Contains(varname)) {
										throw new SemanticError("Duplicated variable: " + varname, node.AnchorToken);
									}
									else{
										TableGlobalVar.Add(varname);
									}
								}
							}
						}
				}

				public void Visit(FunDef node){
						var funcName = node.AnchorToken.Lexeme;
						
						if(noOfPass == 1){
							if(TableFunc.ContainsKey(funcName)){
								throw new SemanticError("Duplicated function: " + funcName, node.AnchorToken);
							}
							else{
									var ariedad = 0;

									if(node[0].hasChildren){
										ariedad = node[0][0].lengthChildren;
									}
									
									TableFunc[funcName] = new FunctionProperties(false, ariedad, new HashSet<string>());
									
							}
						}
						else{
							currentFuncName = funcName;

							var parmas = Visit((dynamic) node[0]);

							TableFunc[currentFuncName].simbolicTable = parmas;

							//Add local variables
							Visit((dynamic) node[1]);

							//StmtList
							Visit((dynamic) node[2]);

							currentFuncName = "";
						}
				}

				public HashSet<string> Visit(ParametersList node){
					var localVariables = new HashSet<string>();

					//Add params to local variables
					if(node.hasChildren){
						foreach(var parameters in node[0]){
							localVariables.Add(parameters.AnchorToken.Lexeme);
						}
					}

					return localVariables;
				}

				public void Visit(VarDefList node){
					var localVariables = TableFunc[currentFuncName].simbolicTable;

					if(node.hasChildren){
						foreach(var nodeVarDef in node){

							foreach(var nodeIdentifier in nodeVarDef[0]){
								if(localVariables.Contains(nodeIdentifier.AnchorToken.Lexeme)){
									throw new SemanticError("Duplicated variable: " + nodeIdentifier.AnchorToken.Lexeme, nodeIdentifier.AnchorToken);
								}
								else{
									localVariables.Add(nodeIdentifier.AnchorToken.Lexeme);
								}	
							}
						}
					}
				}

				public void Visit(Assign node){
					Visit((dynamic) node[0]);
				}

				public void Visit(FunCall node){
					var funName = node.AnchorToken.Lexeme;

					if(!TableFunc.ContainsKey(funName)){
						throw new SemanticError("Undeclared function: " + funName, node[0].AnchorToken);
					}
					else{
						var ariety = TableFunc[funName].ariety;

						if(node[0].lengthChildren != ariety){
							throw new SemanticError("Expected parameters for function " + funName + ": " + ariety + ". Got: " + node[0].lengthChildren, 
													node.AnchorToken);
						}
					}

					VisitChildren(node);
				}

				public void Visit(StmtIncr node){
					var localVar = TableFunc[currentFuncName].simbolicTable;
					var varName = node[0].AnchorToken.Lexeme;

					if(!localVar.Contains(varName)){
						if(!TableGlobalVar.Contains(varName)){
							throw new SemanticError("Undeclared variable: " + varName, node[0].AnchorToken);
						}
					}	
				}

				public void Visit(StmtDecr node){
					var localVar = TableFunc[currentFuncName].simbolicTable;
					var varName = node[0].AnchorToken.Lexeme;

					if(!localVar.Contains(varName)){
						if(!TableGlobalVar.Contains(varName)){
							throw new SemanticError("Undeclared variable: " + varName, node[0].AnchorToken);
						}
					}	
				}

				public void Visit(IntLiteral node){
					try{
                		Int32.Parse(node.AnchorToken.Lexeme);
           			} 
					catch{
                		throw new SemanticError("Int out of bounds (32 bits): ", node.AnchorToken); 
    				}
				}

				public void Visit(StmtWhile node){
            		lvlLoop += 1;
            		VisitChildren(node);
            		lvlLoop -= 1;
       		 	}

				public void Visit(StmtDoWhile node){
					lvlLoop += 1;
					VisitChildren(node);
					lvlLoop -= 1;
				}

				public void Visit(Break node){
					if (lvlLoop == 0){
						throw new SemanticError("Break statement not properly used: ", node.AnchorToken);
					}
				}

				public void Visit(Division node){
					var denom = node[1].AnchorToken.Lexeme;

					if(node[1] is IntLiteral){
						Visit((dynamic) node[1]);

						if(denom == "0"){
							throw new SemanticError("Division by 0: ", node.AnchorToken);
						}
					}
				}

				public void Visit(Module node){
					var denom = node[1].AnchorToken.Lexeme;

					if(node[1] is IntLiteral){
						Visit((dynamic) node[1]);

						if(denom == "0"){
							throw new SemanticError("Division by 0: ", node.AnchorToken);
						}
					}
				}

				public void Visit(Less node){
					VisitChildren((dynamic) node);
				}

				public void Visit(Plus node){
					VisitChildren((dynamic) node);
				}

				public void Visit(Multiply node){
					VisitChildren((dynamic) node);
				}

				public void Visit(LessThan node){
					VisitChildren((dynamic) node);
				}

				public void Visit(LessEqualThan node){
					VisitChildren((dynamic) node);
				}

				public void Visit(MoreThan node){
					VisitChildren((dynamic) node);
				}

				public void Visit(MoreEqualThan node){
					VisitChildren((dynamic) node);
				}

				public void Visit(Compare node){
					VisitChildren((dynamic) node);
				}

				public void Visit(Different node){
					VisitChildren((dynamic) node);
				}

				public void Visit(StmtList node){
					VisitChildren((dynamic) node);
				}

				public void Visit(ExprList node){
					VisitChildren((dynamic) node);
				}

				public void Visit(ExprUnary node){
					VisitChildren((dynamic) node);
				}

				public void Visit(Identifier node){
					var localVar = TableFunc[currentFuncName].simbolicTable;

					var varName = node.AnchorToken.Lexeme;

					if(!localVar.Contains(varName)){
						if(!TableGlobalVar.Contains(varName)){
							throw new SemanticError("Undeclared variable: " + varName, node.AnchorToken);
						}
					}
				}

				public void Visit(StmtIf node){
					VisitChildren((dynamic) node);
				}

				public void Visit(Else node){
					VisitChildren((dynamic) node);
				}

				public void Visit(Elif node){
					VisitChildren((dynamic) node);
				}

				public void Visit(ExprOr node){
					VisitChildren((dynamic) node);
				}

				public void Visit(ExprAnd node){
					VisitChildren((dynamic) node);
				}

				public void Visit(ElseIfList node){
					VisitChildren((dynamic) node);
				}

				public void Visit(StmtReturn node){
					VisitChildren((dynamic) node);
				}

				public void Visit(IdList node){
					VisitChildren((dynamic) node);
				}

				public void Visit(CharLit node){
					//Does nothing
				}

				public void Visit(StringLit node){
					//Does nothing
				}

				public void Visit(Not node){
					//Does nothing
				}

				public void Visit(True node){
					//Does nothing
				}

				public void Visit(False node){
					//Does nothing
				}

				public void Visit(Empty node){
					//Does nothing
				}

				public void VisitChildren(Node node){
					foreach (var n in node) {
                		Visit((dynamic) n);
            		}
				}
		}
	
}
