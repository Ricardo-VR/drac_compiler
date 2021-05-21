namespace drac {

    class DefList: Node{} //
    class VarDef: Node{} //la pusimos
    class IdList: Node{} //no
    class Identifier: Node {} //no
    class FunDef: Node {} // si
    class VarDefList: Node {} //
    class StmtList: Node {} //
    class StmtIncr: Node {} //
    class StmtDecr: Node {} //
    class ExprList: Node {} //si
    class StmtIf: Node {} //no
    class ElseIfList: Node {} //no
    class Elif: Node{} //no
    class Else: Node {} //no
    class StmtWhile: Node {} //
    class StmtDoWhile: Node {} //
    class Break: Node {} //
    class StmtReturn: Node {} //no
    class Empty: Node {} //no
    class ExprOr: Node {} //no
    class ExprAnd: Node {} //no
    class Compare: Node {} //no
    class Different: Node {} //no
    class LessThan: Node {} //no
    class LessEqualThan: Node {} //no
    class MoreThan: Node {} //no
    class MoreEqualThan: Node {} //no
    class Plus: Node {} //no
    class Less: Node {}//no
    class Multiply: Node {}
    class Division: Node {} //
    class Module: Node {} // si
    class ExprUnary: Node {}
    class Not: Node {} //no
    class True: Node {} //no
    class False: Node {} //no
    class IntLiteral: Node {} // si
    class CharLit: Node {} //no
    class StringLit: Node {} //no
    class Assign : Node {} //
    class FunCall : Node{} //
    class ParametersList : Node{} //TODO
}
