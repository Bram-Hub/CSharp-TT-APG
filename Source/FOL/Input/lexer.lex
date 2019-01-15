%namespace TruthTree2.FOL.Input

%%

"and"					{ return (int)Tokens.And; }
"or"					{ return (int)Tokens.Or; }
"not"					{ return (int)Tokens.Not; }
"iff"					{ return (int)Tokens.Iff; }
"if"					{ return (int)Tokens.If; }
"forall"				{ return (int)Tokens.Forall; }
"exists"				{ return (int)Tokens.Exists; }
"="						{ return (int)Tokens.Equals; }

"\("					{ return (int)Tokens.LParen; }
"\)"					{ return (int)Tokens.RParen; }
"\["					{ return (int)Tokens.LParen; }
"\]"					{ return (int)Tokens.RParen; }
"\{"					{ return (int)Tokens.LParen; }
"\}"					{ return (int)Tokens.RParen; }

"false"					{ return (int)Tokens.False; }

[a-zA-Z][a-zA-Z0-9_]*	{ yylval.stringval = yytext; return (int)Tokens.Identifier; }
_[a-zA-Z][a-zA-Z0-9_]*	{ yylval.stringval = yytext.Substring(1); return (int)Tokens.Variable; }

[\n\r\t ]				{ /* Ignore whitespace */ }

.						{ /* Ignore unknown */ }

%%