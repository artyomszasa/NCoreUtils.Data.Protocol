grammar Protocol;

/* PARSER */

start:
    lambda EOF
    | expr EOF
    ;

lambda: IDENT ARROW expr;

expr:
    LPAREN expr RPAREN
    | ident
    | constant
    | call
    | expr binOp=(MUL | DIV | MOD) expr
    | expr binOp=(PLUS | MINUS) expr
    | expr binOp=(EQ | NEQ | GT | LT | GE | LE) expr
    | expr binOp=AND expr
    | expr binOp=OR expr
    | lambda
    ;

args: expr (COMMA expr)*;

ident: IDENT (DOT IDENT)*;

call: IDENT LPAREN args RPAREN;

constant: numValue | stringValue;

numValue: NUMVALUE;

stringValue: STRINGVALUE;

/* LEXER */

fragment NUM: [0-9];
fragment ALPHA: [a-zA-Z_];
fragment ALPHANUM: (NUM|ALPHA);
fragment QUOTE: '"';
fragment NOT_QUOTE: ~'"';
fragment ESCAPEDQUOTE: '\\"';

WS: (' '|'\t')+ -> skip;
DOT: '.';
AND: '&&';
OR: '||';
EQ: '=';
NEQ: '!=';
LE: '<=';
GE: '>=';
LT: '<';
GT: '>';
LPAREN: '(';
RPAREN: ')';
COMMA: ',';
ARROW: '=>';
PLUS: '+';
MINUS: '-';
DIV: '/';
MUL: '*';
MOD: '%';

NUMVALUE: NUM+('.'NUM+)?;
STRINGVALUE: QUOTE (ESCAPEDQUOTE|NOT_QUOTE)* QUOTE;
IDENT: ALPHA (ALPHANUM)*;