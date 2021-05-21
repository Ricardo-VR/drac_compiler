/*Authors:
Natalia Meraz Tostado A01745008
Humberto Perez Vargas A01651926
Ricardo Velazquez Rios A01746958
*/

//<program> 				::= <def-list>
// <def-list> 				::=<def>*
// <def> 					::= <var-def> | <fun-def>
// <var-def> 				::= var <id-list>;
// <id-list> 				::= id <id-list-cont>
// <id-list-cont> 			::= (, id) * 
// <fun-def> 				::= id "(" <param-list> ")" "{" <var-def-list> <stmt-list> "}"
// <param-list> 			::= <id-list>?
// <var-def-list> 			::= <var-def>*
// <stmt-list> 				::=  <stmt> *
// <stmt> 					::= <stmt-assign> | <stmt-incr> |  <stmt-decr> | <stmt-fun-call> | <stmt-if> | <stmt-while> | <stmt-do-while> | <stmt-break> | <stmt-return> | <stmt-empty>
// <stmt-assign> 			::= id = <expr> ;
// <stmt-incr> 				::= inc id ;
// <stmt-decr> 				::= dec id ;
// <stmt-fun-call> 			::= <fun-call> ;
// <fun-call> 				::= id "(" <expr-list> ")"
// <expr-list>  			::=  (<expr> <expr-list-cont>)?
// <expr-list-cont> 		::=  ( , <expr> )* 
// <stmt-if>  				::=  if "(" <expr> ")" "{" <stmt-list> "}" <else-if-list> <else>
// <else-if-list>  			::=  ( elif "(" <expr> ")" "{" <stmt-list> "}" ) *	
// <else>  					::=  ( else "{" <stmt-list> "}" )?	
// <stmt-while>  			::=  while "(" <expr> ")" "{" <stmt-list> "}"
// <stmt-do-while>  		::=  do "{" <stmt-list> "}" while "(" <expr> ")" ;
// <stmt-break>  			::=  break ;
// <stmt-return>  			::=  return <expr> ;
// <stmt-empty>  			::=  ;
// <expr>  					::=  <expr-or>
// <expr-or>  				::=  <expr-and> (or <expr-and>)* 
// <expr-and>  				::=  <expr-comp> (and <expr-comp>)*
// <expr-comp> 				::=  <expr-rel> (<op-comp> <expr-rel>)*
// <op-comp>  				::=  == | <> 
// <expr-rel>  				::= <expr-add> (<op-rel> <expr-add>)*
// <op-rel> 				::= < | <= | > | >=
// <expr-add> 				::=  <expr-mul> (<op-add> <expr-mul>)*
// <op-add> 				::= + | - 
// <expr-mul> 				::= <expr-unary> (<op-mul> <expr-unary>)*
// <op-mul> 				::= * | / | %
// <expr-unary> 			::= <op-unary>* <expr-primary>
// <op-unary> 				::= + | - | not
// <expr-primary> 			::= id | <fun-call> | <array> | <lit> | "(" <expr> ")"
// <array> 					::= "[" <expr-list> "]"
// <lit> 					::= <lit-bool> | <lit-int> | <lit-char> | <lit-str>

using System;
using System.Collections.Generic;

namespace drac {

		class Parser {
				
				static readonly ISet<TokenCategory> firstOfDef =
						new HashSet<TokenCategory>() {
						TokenCategory.VAR,
						TokenCategory.IDENTIFIER
				};
				
				static readonly ISet<TokenCategory> firstOfStmt =
						new HashSet<TokenCategory>() {
						TokenCategory.IDENTIFIER,
						TokenCategory.INC,
						TokenCategory.DEC,
						TokenCategory.IF,
						TokenCategory.WHILE,
						TokenCategory.DO,
						TokenCategory.BREAK,
						TokenCategory.RETURN,
						TokenCategory.SEMICOLON
				};
				
				static readonly ISet<TokenCategory> firstOfOpComp =
						new HashSet<TokenCategory>() {
						TokenCategory.COMPARE,
						TokenCategory.DIFFERENT
				};
				
				static readonly ISet<TokenCategory> firstOfOpRel =
						new HashSet<TokenCategory>() {
						TokenCategory.LESS_THAN,
						TokenCategory.LESS_EQUAL_THAN,
						TokenCategory.MORE_THAN,
						TokenCategory.MORE_EQUAL_THAN
				};
				
				static readonly ISet<TokenCategory> firstOfOpAdd =
						new HashSet<TokenCategory>() {
						TokenCategory.PLUS,
						TokenCategory.LESS
				};

				static readonly ISet<TokenCategory> firstOfOpMul =
						new HashSet<TokenCategory>() {
						TokenCategory.MULTIPLY,
						TokenCategory.DIVISION,
						TokenCategory.MODULE
				};

				static readonly ISet<TokenCategory> firstOfOpUnary =
						new HashSet<TokenCategory>() {
						TokenCategory.PLUS,
						TokenCategory.LESS,
						TokenCategory.NOT
				};

				static readonly ISet<TokenCategory> firstOfExprPrim =
						new HashSet<TokenCategory>() {
						TokenCategory.IDENTIFIER,
						TokenCategory.OPEN_S_BRACKET,
						TokenCategory.TRUE,
						TokenCategory.FALSE,
						TokenCategory.INT_LITERAL,
						TokenCategory.CHAR_LIT,
						TokenCategory.STRING_LIT,
						TokenCategory.OPEN_PARENTHESIS
				};

				static readonly ISet<TokenCategory> firstOfLit =
						new HashSet<TokenCategory>() {
						TokenCategory.TRUE,
						TokenCategory.FALSE,
						TokenCategory.INT_LITERAL,
						TokenCategory.CHAR_LIT,
						TokenCategory.STRING_LIT
				};
				

				IEnumerator<Token> tokenStream;

				public Parser(IEnumerator<Token> tokenStream) {
						this.tokenStream = tokenStream;
						this.tokenStream.MoveNext();
				}

				public TokenCategory CurrentToken {
						get { return tokenStream.Current.Category; }
				}

				public Token Expect(TokenCategory category) {
						if (CurrentToken == category) {
								Token current = tokenStream.Current;
								tokenStream.MoveNext();
								return current;
						} else {
								throw new SyntaxError(category, tokenStream.Current);
						}
				}
				
				public Node program() {
						return def_list();
						
				}

				public Node def_list() {
						var defList = new DefList();
						while  (firstOfDef.Contains(CurrentToken)) {
                				defList.Add(def());
						}
						return defList;
				}

				public Node def(){
						switch(CurrentToken)
						{
								case TokenCategory.VAR:
										var def = var_def();;
										return def;

								case TokenCategory.IDENTIFIER:
										return fun_def();
								default:
										throw new SyntaxError(firstOfDef, tokenStream.Current);
						}
				}

				public Node var_def(){
						var varDef = new VarDef();
						Expect(TokenCategory.VAR);
						varDef.Add(id_list());
						Expect(TokenCategory.SEMICOLON);
						return varDef;
				}

				// quitamos var_list ---------------------------------------------------------------------------------------------------

				public Node id_list(){
						var idList = new IdList();
						idList.Add(new Identifier(){
							AnchorToken = Expect(TokenCategory.IDENTIFIER)
						});

						while(CurrentToken == TokenCategory.COMMA){
								Expect(TokenCategory.COMMA);
								var ident = new Identifier(){
									AnchorToken = Expect(TokenCategory.IDENTIFIER)
								};
								idList.Add(ident);
						}
						return idList;
				}

				public Node fun_def(){
						var funDef = new FunDef(){
							AnchorToken = Expect(TokenCategory.IDENTIFIER)
						};
						
						Expect(TokenCategory.OPEN_PARENTHESIS);

						funDef.Add(param_list());

						Expect(TokenCategory.CLOSE_PARENTHESIS);
						Expect(TokenCategory.OPEN_BRACKET);
						funDef.Add(var_def_list());
						funDef.Add(stmt_list());
						Expect(TokenCategory.CLOSE_BRACKET);
						return funDef;
				}
				
				public Node param_list(){
						var paramList = new ParametersList();

						Console.WriteLine(CurrentToken);

						if(CurrentToken == TokenCategory.IDENTIFIER){
							paramList.Add(id_list());
						}

						return paramList;
				}

				public Node var_def_list(){
						var varDefList = new VarDefList();
						while(CurrentToken == TokenCategory.VAR){
								varDefList.Add(var_def());
						}
						return varDefList;
				}
				public Node stmt_list(){
						var stmtList = new StmtList();
						while(firstOfStmt.Contains(CurrentToken)){						
							stmtList.Add(stmt());
						}
						return stmtList;
				}

				public Node stmt(){
							switch (CurrentToken) {
									case TokenCategory.IDENTIFIER:
										var ident = new Identifier(){
											AnchorToken = Expect(TokenCategory.IDENTIFIER)
										};

										if(CurrentToken == TokenCategory.OPEN_PARENTHESIS){ //stmt_fun_call
												var result = stmt_fun_call(ident);
												return result;
										}
										else{
											var nodeAssign = stmt_assign(ident);
											return nodeAssign;
										}
									case TokenCategory.INC:
											return stmt_incr();
									case TokenCategory.DEC:
											return stmt_dec();
									case TokenCategory.IF:
											return stmt_if();
									case TokenCategory.WHILE:
											return stmt_while();
									case TokenCategory.DO:
											return stmt_do_while();
									case TokenCategory.BREAK:
											return stmt_break();
									case TokenCategory.RETURN:
											return stmt_return();
									case TokenCategory.SEMICOLON:
											return stmt_empty();
									default:
											throw new SyntaxError(firstOfStmt, tokenStream.Current);
							}
				}

				public Node stmt_assign(Identifier node){
						Expect(TokenCategory.ASSIGN);

						var nodeAssign = new Assign(){
							AnchorToken = node.AnchorToken		//quitamos .lexeme------------------------------------------------------------
						};

						var expression = expr();
						nodeAssign.Add(expression);
						Expect(TokenCategory.SEMICOLON);
						return nodeAssign;
				}
				public Node stmt_incr(){
						var stmtIncr = new StmtIncr(){
							AnchorToken = Expect(TokenCategory.INC)
						};
						
						var ident = new Identifier(){
							AnchorToken = Expect(TokenCategory.IDENTIFIER)
						};
						stmtIncr.Add(ident);
						Expect(TokenCategory.SEMICOLON);
						return stmtIncr;
				}
				
				public Node stmt_dec(){
						var stmtDecr = new StmtDecr(){
							AnchorToken = Expect(TokenCategory.DEC)
						};
						var ident = new Identifier(){
							AnchorToken = Expect(TokenCategory.IDENTIFIER)
						};
						stmtDecr.Add(ident);
						Expect(TokenCategory.SEMICOLON);
						return stmtDecr;
				}
				
				public Node stmt_fun_call(Identifier ident){
						var stmtFunCall = funcall(ident);
						Expect(TokenCategory.SEMICOLON);

						return stmtFunCall;
				}

				public Node funcall(Identifier ident){
						var result = new FunCall(){
							AnchorToken = ident.AnchorToken
						};

						Expect(TokenCategory.OPEN_PARENTHESIS);
						result.Add(expr_list());
						Expect(TokenCategory.CLOSE_PARENTHESIS);
						return result;
				}
				
				public Node expr_list(){
						var exprList = new ExprList();

						if(firstOfExprPrim.Contains(CurrentToken)){
							exprList.Add(expr());

							while(CurrentToken == TokenCategory.COMMA){
								Expect(TokenCategory.COMMA);
								exprList.Add(expr());
							}
						}

						if(firstOfOpUnary.Contains(CurrentToken)){
							exprList.Add(expr_unary());

							while(CurrentToken == TokenCategory.COMMA){
								Expect(TokenCategory.COMMA);
								exprList.Add(expr());
							}
						}
						return exprList;
				}

				public Node stmt_if(){
						var ifToken = Expect(TokenCategory.IF);
						Expect(TokenCategory.OPEN_PARENTHESIS);

						var expression = expr();
						Expect(TokenCategory.CLOSE_PARENTHESIS);
						Expect(TokenCategory.OPEN_BRACKET);

						var stmtList = stmt_list();
						Expect(TokenCategory.CLOSE_BRACKET);

						var elseIfList = else_if_list();
						var elseToken = else_();
						var stmtIf = new StmtIf(){expression, stmtList, elseIfList, elseToken};
						stmtIf.AnchorToken = ifToken;
						return stmtIf;
				}

				public Node else_if_list(){
						var elseIfList = new ElseIfList();
						while(CurrentToken == TokenCategory.ELIF){
							var elifToken = Expect(TokenCategory.ELIF);
							Expect(TokenCategory.OPEN_PARENTHESIS);
							var expression = expr();
							Expect(TokenCategory.CLOSE_PARENTHESIS);
							Expect(TokenCategory.OPEN_BRACKET);
							var stmtList = stmt_list();
							Expect(TokenCategory.CLOSE_BRACKET);
							var elif = new Elif(){expression, stmtList};
							elif.AnchorToken = elifToken;
							elseIfList.Add(elif);
						}
						return elseIfList;
				}

				public Node else_(){
						var else_ = new Else();
						if(CurrentToken == TokenCategory.ELSE){
							var elseToken = Expect(TokenCategory.ELSE);
							Expect(TokenCategory.OPEN_BRACKET);
							var stmtList = stmt_list();
							Expect(TokenCategory.CLOSE_BRACKET);
							else_.AnchorToken = elseToken;
							else_.Add(stmtList);
						}
						return else_;
				}
				
				public Node stmt_while(){
						var whileToken = Expect(TokenCategory.WHILE);
						Expect(TokenCategory.OPEN_PARENTHESIS);
						var expression = expr();
						Expect(TokenCategory.CLOSE_PARENTHESIS);
						Expect(TokenCategory.OPEN_BRACKET);
						var stmtList = stmt_list();
						Expect(TokenCategory.CLOSE_BRACKET);
						var stmtWhile = new StmtWhile(){expression, stmtList};
						stmtWhile.AnchorToken = whileToken;
						return stmtWhile;
				}

				public Node stmt_do_while(){
						var doToken = Expect(TokenCategory.DO);
						Expect(TokenCategory.OPEN_BRACKET);
						var stmtList = stmt_list();
						Expect(TokenCategory.CLOSE_BRACKET);
						Expect(TokenCategory.WHILE);
						Expect(TokenCategory.OPEN_PARENTHESIS);
						var expression = expr();
						Expect(TokenCategory.CLOSE_PARENTHESIS);
						Expect(TokenCategory.SEMICOLON);
						var stmtDoWhile = new StmtDoWhile(){stmtList, expression};
						stmtDoWhile.AnchorToken = doToken;
						return stmtDoWhile;
				}
				
				public Node stmt_break(){
						var tokenBreak = Expect(TokenCategory.BREAK);
						Expect(TokenCategory.SEMICOLON);
						return new Break(){
							AnchorToken = tokenBreak
						};
				}
				
				public Node stmt_return(){
						var stmtReturn = new StmtReturn(){
							AnchorToken = Expect(TokenCategory.RETURN)
						};
						stmtReturn.Add(expr());
						Expect(TokenCategory.SEMICOLON);
						return stmtReturn;
				}

				public Node stmt_empty(){
						return new Empty(){
							AnchorToken = Expect(TokenCategory.SEMICOLON)
						};
				}

				public Node expr(){
						return expr_or();
				}
				
				public Node expr_or(){
						var currentRoot = expr_and();

						while(CurrentToken == TokenCategory.OR){
								var nodeOr = new ExprOr() {
									AnchorToken = Expect(TokenCategory.OR)
								};
								nodeOr.Add(currentRoot);
								currentRoot = nodeOr;

								var nodeExprAnd = expr_and();
								currentRoot.Add(nodeExprAnd);
						}
						return currentRoot;
				}

				public Node expr_and(){
						var currentRoot = expr_comp();

						while(CurrentToken == TokenCategory.AND){
								var nodeAnd = new ExprAnd() {
									AnchorToken = Expect(TokenCategory.AND)
								};
								nodeAnd.Add(currentRoot);
								currentRoot = nodeAnd;

								var nodeExprComp2 = expr_comp();
								currentRoot.Add(nodeExprComp2);
						}
						return currentRoot;
				}

				public Node expr_comp(){
						var currentRoot = expr_rel();

						while(firstOfOpComp.Contains(CurrentToken)){
							var nodeOpComp = op_comp();
							nodeOpComp.Add(currentRoot);
							currentRoot = nodeOpComp;

							var nodeExprRel2 = expr_rel();
							currentRoot.Add(nodeExprRel2);
						}
						return currentRoot;
				}

				public Node op_comp(){
						switch (CurrentToken){
								case TokenCategory.COMPARE:
									return new Compare(){
										AnchorToken = Expect(TokenCategory.COMPARE)
									};
								case TokenCategory.DIFFERENT:
									return new Different(){
										AnchorToken = Expect(TokenCategory.DIFFERENT)
									};
								default:
										throw new SyntaxError(firstOfOpComp, tokenStream.Current);
						}
				}

				public Node expr_rel(){
						var exprRel1 = expr_add();

						while(firstOfOpRel.Contains(CurrentToken)){
							var exprRel2 = op_rel();
							exprRel2.Add(exprRel1);
							exprRel2.Add(expr_add());
							exprRel1 = exprRel2;
						}
						return exprRel1;
					}

				public Node op_rel(){
						switch(CurrentToken){
								case TokenCategory.LESS_THAN:
									return new LessThan(){
										AnchorToken = Expect(TokenCategory.LESS_THAN)
									};
								case TokenCategory.LESS_EQUAL_THAN:
									return new LessEqualThan(){	
										AnchorToken = Expect(TokenCategory.LESS_EQUAL_THAN)
									};
								case TokenCategory.MORE_THAN:
									return new MoreThan(){
										AnchorToken = Expect(TokenCategory.MORE_THAN)
									};
								case TokenCategory.MORE_EQUAL_THAN:
									return new MoreEqualThan(){	
										AnchorToken = Expect(TokenCategory.MORE_EQUAL_THAN)
									};
								default:
										throw new SyntaxError(firstOfOpRel, tokenStream.Current);
						}
				}

				public Node expr_add(){
						var exprAdd1 = expr_mul();

						while(firstOfOpAdd.Contains(CurrentToken)){
							var exprAdd2 = op_add();
							exprAdd2.Add(exprAdd1);
							exprAdd2.Add(expr_mul());
							exprAdd1 = exprAdd2;
						}
					return exprAdd1;
				}

				public Node op_add(){
						switch (CurrentToken){
								case TokenCategory.PLUS:
									return new Plus(){
										AnchorToken = Expect(TokenCategory.PLUS)
									};
								case TokenCategory.LESS:
									return new Less(){
										AnchorToken = Expect(TokenCategory.LESS)
									};
								default:
										throw new SyntaxError(firstOfOpAdd, tokenStream.Current);
						}
				}

				public Node expr_mul(){
						var exprMul1 = expr_unary();

						while(firstOfOpMul.Contains(CurrentToken)){
							var exprMul2 = op_mul();
							exprMul2.Add(exprMul1);
							exprMul2.Add(expr_unary());
							exprMul1 = exprMul2;
						}
					return exprMul1;
				}

				public Node op_mul(){
						switch(CurrentToken){
								case TokenCategory.MULTIPLY:
									return new Multiply(){
										AnchorToken = Expect(TokenCategory.MULTIPLY)
									};
								case TokenCategory.DIVISION:
									return new Division(){
										AnchorToken = Expect(TokenCategory.DIVISION)
									};
								case TokenCategory.MODULE:
									return new Module(){
										AnchorToken = Expect(TokenCategory.MODULE)
									};
								default:
										throw new SyntaxError(firstOfOpMul, tokenStream.Current);
						}
				}

				public Node expr_unary(){
						var expressionUnary = new ExprUnary();
						var intNumChildren = 0;

						while(firstOfOpUnary.Contains(CurrentToken)){
							expressionUnary.Add(op_unary());
							intNumChildren++;
						}

						var nodeExprPrim = expr_primary();

						if(intNumChildren > 0){
							expressionUnary.Add(nodeExprPrim);
							return expressionUnary;
						}
						else{
							return nodeExprPrim;
						}
						
						
				}

				public Node op_unary(){
						switch(CurrentToken){
								case TokenCategory.PLUS:
									return new Plus(){
										AnchorToken = Expect(TokenCategory.PLUS)
									};
								case TokenCategory.LESS:
									return new Less(){
										AnchorToken = Expect(TokenCategory.LESS)
									};
								case TokenCategory.NOT:
									return new Not(){
										AnchorToken = Expect(TokenCategory.NOT)
									};
								default:
										throw new SyntaxError(firstOfOpUnary, tokenStream.Current);
						}
				}
				
				public Node expr_primary(){
						switch(CurrentToken){
								case TokenCategory.IDENTIFIER:
										var ident = new Identifier(){
												AnchorToken = Expect(TokenCategory.IDENTIFIER)
										};
	
										if(CurrentToken == TokenCategory.OPEN_PARENTHESIS){ //fun_call
											var result = funcall(ident);

											return result;
										}

										return ident;
								case TokenCategory.OPEN_S_BRACKET:
										return array();
								case TokenCategory.TRUE:									
										return lit();
								case TokenCategory.FALSE:									
										return lit();
								case TokenCategory.INT_LITERAL:									
										return lit();
								case TokenCategory.CHAR_LIT:
										return lit();
								case TokenCategory.STRING_LIT:
										return lit();
								case TokenCategory.OPEN_PARENTHESIS:
										Expect(TokenCategory.OPEN_PARENTHESIS);
										var expression = expr();
										Expect(TokenCategory.CLOSE_PARENTHESIS);
										return expression;		
								default:
										throw new SyntaxError(firstOfExprPrim, tokenStream.Current);
						}
				}
				public Node array(){
						Expect(TokenCategory.OPEN_S_BRACKET);
						var exprList = expr_list();
						Expect(TokenCategory.CLOSE_S_BRACKET);
						return exprList;
				}

				public Node lit(){
						switch(CurrentToken){
								case TokenCategory.TRUE:
									return new True(){
										AnchorToken = Expect(TokenCategory.TRUE)
									};
								case TokenCategory.FALSE:
									return new False(){
										AnchorToken = Expect(TokenCategory.FALSE)
									};
								case TokenCategory.INT_LITERAL:
									return new IntLiteral(){
										AnchorToken = Expect(TokenCategory.INT_LITERAL)
									};
								case TokenCategory.CHAR_LIT:
									return new CharLit(){
										AnchorToken = Expect(TokenCategory.CHAR_LIT)
									};
								case TokenCategory.STRING_LIT:
									return new StringLit(){
										AnchorToken = Expect(TokenCategory.STRING_LIT)
									};
								default:
										throw new SyntaxError(firstOfLit, tokenStream.Current);
						}
				}
		
		}
}