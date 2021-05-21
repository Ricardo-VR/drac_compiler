using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace drac{

  class Scanner {

  readonly string input;

  static readonly Regex regex = new Regex(
  @"

    (?<Comp>                [=][=]    )
    | (?<Assig>             [=]       )     # Must go after Newline.
    | (?<M_Comm>            ([(][*](.|\s)*?[*][)])  ) 
    | (?<S_Comm>            [-][-].* )
    | (?<And>               and\b )
    | (?<Or>                or\b )
    | (?<Not>               not\b )
    | (?<If>                if\b )
    | (?<Inc>               inc\b )
    | (?<Elif>              elif\b )
    | (?<Else>              else\b )
    | (?<Do>                do\b )
    | (?<While>             while\b )
    | (?<Break>             break\b )
    | (?<Return>            return\b )
    | (?<True>              true\b )
    | (?<False>             false\b )
    | (?<Var>               var\b )
    | (?<Dec>               dec\b )
    | (?<Plus>              [+]       )
    | (?<Less>              [-]       )
    | (?<Prod>              [*]       )
    | (?<Div>               [/]       )
    | (?<Mod>               [%]       )
    | (?<ParLeft>           [(]       )
    | (?<ParRight>          [)]       )
    | (?<LeftCBrace>        [{]      )
    | (?<RightCBrace>       [}]      )
    | (?<LeftSBrace>        [\[]      )
    | (?<RightSBrace>       [\]]      )
    | (?<LessEqualThan>     [<][=] )
    | (?<MoreEqualThan>     [>][=] )
    | (?<Different>         [<][>] )
    | (?<LessThan>          [<]       )
    | (?<MoreThan>          [>]       )
    | (?<Comma>             [,]       )
    | (?<SemiColon>         [;]       )
    | (?<Char_Lit>          [']([\\]([nrt\'\\""\\]|u[\dA-Fa-f]{6})|[^\\])['] )
    | (?<String_Lit>        [""]([^\\""]|[\\].)*?[""] )
    | (?<Unicode>           [\\][u][A-Fa-f\d]{6})
    | (?<Backslash>         [\\]     )
    | (?<Newline>           \n       )
    | (?<Whitespace>        \s       )
    | (?<IntLiteral>        \d+      )
    | (?<Identifier>        [a-zA-Z]+[\d]*([a-zA-z_]*[\d]*)*)
    | (?<Other>             [.]        )
    
    ",
    RegexOptions.IgnorePatternWhitespace
    | RegexOptions.Compiled
    | RegexOptions.Multiline
    );

    static readonly IDictionary<string, TokenCategory> tokenMap =
    new Dictionary<string, TokenCategory>() {
    {"And", TokenCategory.AND},
    {"Or", TokenCategory.OR},
    {"Not", TokenCategory.NOT},
    {"Inc", TokenCategory.INC},
    {"If", TokenCategory.IF},
    {"Elif", TokenCategory.ELIF},
    {"Else", TokenCategory.ELSE},
    {"Do", TokenCategory.DO},
    {"While", TokenCategory.WHILE},
    {"Break", TokenCategory.BREAK},
    {"Return", TokenCategory.RETURN},
    {"True", TokenCategory.TRUE},
    {"False", TokenCategory.FALSE},
    {"Var", TokenCategory.VAR},
    {"Dec", TokenCategory.DEC},
    {"Comp", TokenCategory.COMPARE},
    {"Assig", TokenCategory.ASSIGN},
    {"Less", TokenCategory.LESS},
    {"Plus", TokenCategory.PLUS},
    {"Prod", TokenCategory.MULTIPLY},
    {"Div", TokenCategory.DIVISION},
    {"Mod", TokenCategory.MODULE},
    {"ParLeft", TokenCategory.OPEN_PARENTHESIS},
    {"ParRight", TokenCategory.CLOSE_PARENTHESIS},
    {"LeftCBrace", TokenCategory.OPEN_BRACKET},
    {"RightCBrace", TokenCategory.CLOSE_BRACKET},
    {"LeftSBrace", TokenCategory.OPEN_S_BRACKET},
    {"RightSBrace", TokenCategory.CLOSE_S_BRACKET},
    {"LessEqualThan", TokenCategory.LESS_EQUAL_THAN},
    {"MoreEqualThan", TokenCategory.MORE_EQUAL_THAN},
    {"LessThan", TokenCategory.LESS_THAN},
    {"MoreThan", TokenCategory.MORE_THAN},
    {"Comma", TokenCategory.COMMA},
    {"SemiColon", TokenCategory.SEMICOLON},
    {"Char_Lit", TokenCategory.CHAR_LIT},
    {"String_Lit", TokenCategory.STRING_LIT},
    {"Unicode", TokenCategory.UNICODE},
    {"Backslash", TokenCategory.BACKSLASH},
    {"IntLiteral", TokenCategory.INT_LITERAL},
    {"Different", TokenCategory.DIFFERENT},
    {"Identifier", TokenCategory.IDENTIFIER}
    };

    public Scanner(string input) {
      this.input = input;
    }

    public IEnumerable<Token> Scan() {

    var result = new LinkedList<Token>();
    var row = 1;
    var columnStart = 0;

    foreach (Match m in regex.Matches(input)) {

      if (m.Groups["Newline"].Success) {
          row++;
          columnStart = m.Index + m.Length;

        } else if (m.Groups["Whitespace"].Success || m.Groups["S_Comm"].Success) {

          // Skip white space and comments.
        
        } else if (m.Groups ["M_Comm"].Success){

          //Skip multicomments

          MatchCollection newMatch = Regex.Matches(m.Groups ["M_Comm"].Value, "\n", RegexOptions.Multiline);

          if(newMatch.Count > 0){
              Match lastMatch = newMatch[newMatch.Count - 1];
              row += newMatch.Count;
              columnStart = m.Index + lastMatch.Index + lastMatch.Length;
          }

        } else if (m.Groups["Other"].Success) {

          // Found an illegal character.
          result.AddLast(
          new Token(m.Value,
          TokenCategory.ILLEGAL_CHAR,
          row,
          m.Index - columnStart + 1));
        } else {

          // Must be any of the other tokens.
          result.AddLast(FindToken(m, row, columnStart));
        }
      }

    result.AddLast(
      new Token(null,
      TokenCategory.EOF,
      row,
      input.Length - columnStart + 1));

      return result;
    }

    Token FindToken(Match m, int row, int columnStart) {
      foreach (var name in tokenMap.Keys) {
        if (m.Groups[name].Success) {
          return new Token(m.Value,
          tokenMap[name],
          row,
          m.Index - columnStart + 1);
        }
      }
      throw new InvalidOperationException("regex and tokenMap are inconsistent: " + m.Value);
    }
  }
}
