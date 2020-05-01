namespace NCVC.Parser

open FParsec

module QueryParser =

    type StringAtom =
        | UserId
        | TimeFrame
        | HasTag
        | StringLiteral of string
    and StringExpr =
        | StringAtom of StringAtom
    
    type DecimalAtom =
        | BodyTemperature
        | DecimalLiteral of decimal
    and DecimalExpr =
        | DecimalAtom of DecimalAtom
        | DSum of DecimalExpr * DecimalExpr
        | DSub of DecimalExpr * DecimalExpr
        | DMul of DecimalExpr * DecimalExpr
        | DDiv of DecimalExpr * DecimalExpr

    type IntegerAtom =
        | IntegerLiteral of int
    and IntegerExpr =
        | IntegerAtom of IntegerAtom
        | ISum of IntegerExpr * IntegerExpr
        | ISub of IntegerExpr * IntegerExpr
        | IMul of IntegerExpr * IntegerExpr
        | IDiv of IntegerExpr * IntegerExpr

    type DateAtom =
        | MeasuredDate
        | DateLiteral of int * int * int
        | Today
        | ThisWeek
        | ThisMonth
    and DateExpr =
        | DateAtom of DateAtom
        | AddDate of DateExpr * IntegerExpr
        | ReduceDate of DateExpr * IntegerExpr

    type BooleanAtom =
        | HasError
        | HasWarning
        | IsSubmitted
        | IsInfected
        | True
        | False
        | SEq of StringExpr * StringExpr
        | SNe of StringExpr * StringExpr
        | IEq of IntegerExpr * IntegerExpr
        | INe of IntegerExpr * IntegerExpr
        | IGt of IntegerExpr * IntegerExpr
        | IGe of IntegerExpr * IntegerExpr
        | ILt of IntegerExpr * IntegerExpr
        | ILe of IntegerExpr * IntegerExpr
        | DEq of DecimalExpr * DecimalExpr
        | DNe of DecimalExpr * DecimalExpr
        | DGt of DecimalExpr * DecimalExpr
        | DGe of DecimalExpr * DecimalExpr
        | DLt of DecimalExpr * DecimalExpr
        | DLe of DecimalExpr * DecimalExpr
        | DtEq of DateExpr * DateExpr
        | DtNe of DateExpr * DateExpr
        | DtGt of DateExpr * DateExpr
        | DtGe of DateExpr * DateExpr
        | DtLt of DateExpr * DateExpr
        | DtLe of DateExpr * DateExpr
    and BooleanExpr =
        | BooleanAtom of BooleanAtom
        | Eq of BooleanExpr * BooleanExpr
        | Ne of BooleanExpr * BooleanExpr
        | Neg of BooleanExpr
        | Conj of BooleanExpr * BooleanExpr
        | Disj of BooleanExpr * BooleanExpr
        | Impl of BooleanExpr * BooleanExpr

    and Expr =
        | BooleanExpr of BooleanExpr
        | StringExpr of StringExpr
        | DateExpr of DateExpr
        | DecimalExpr of DecimalExpr
        | IntegerExpr of IntegerExpr
    

    type SortingAttribute =
        | UserId
        | TimeFrame
        | MeasuredDate
        | BodyTemperature
        | HasError
        | HasWarning
        | IsSubmitted
        | IsInfected
    type SortingOrder =
        | Asc
        | Desc

    type Ordering = SortingAttribute * SortingOrder
    type Query = (BooleanExpr list * (Ordering list) option)

    //------

    type UserState = unit

    let pstring_ws s : Parser<string, UserState> = pstring s .>> spaces
    let pinteger_ws : Parser<int, UserState> = pint16 |>> int .>> spaces
    let pbetweenParentheses_ws p = pchar '(' >>. spaces >>. p .>> spaces .>> pchar ')' .>> spaces


    let pexpr, pexprImpl = createParserForwardedToRef ()

    // String parser

    let pstringUserId : Parser<StringAtom, UserState> = stringReturn "user" StringAtom.UserId
    let pstringTimeFrame : Parser<StringAtom, UserState> = stringReturn "timeframe" StringAtom.TimeFrame
    let pstringHasTag : Parser<StringAtom, UserState> = stringReturn "tag" StringAtom.HasTag

    let pstringLiteral : Parser<(StringAtom), UserState> =
        manyChars (noneOf ['"']) |> between (pchar '"') (pchar '"') .>> spaces |>> string |>> StringLiteral
    let pstringAtom = 
        (attempt pstringUserId <|> attempt pstringTimeFrame <|> attempt pstringLiteral <|> pstringHasTag) .>> spaces
    let pstringExpr = pstringAtom |>> StringAtom


    // Integer parser

    let pintegerLiteral : Parser<(IntegerAtom), UserState> = pint16 |>> int |>> IntegerLiteral

    let pintegerAtom : Parser<IntegerAtom, UserState> =
        pintegerLiteral .>> spaces

    let pintegerOp1 =
        choice
            [ pchar '+' .>> spaces >>% (fun x y -> ISum(x, y) )
              pchar '-' .>> spaces >>% (fun x y -> ISub(x, y) ) ]
    let pintegerOp2 =
        choice
            [ pchar '*' .>> spaces >>% (fun x y -> IMul(x, y) )
              pchar '\\' .>> spaces >>% (fun x y -> IDiv(x, y) ) ]
        
    let pintegerExpr, pintegerExprImpl = createParserForwardedToRef ()
    let pintegerFactor : Parser<IntegerExpr, UserState> =
        attempt (pbetweenParentheses_ws pintegerExpr)
        <|> (pintegerAtom |>> IntegerAtom)
    let pintegerTerm : Parser<IntegerExpr, UserState> =
        chainl1 pintegerFactor pintegerOp2
    pintegerExprImpl :=
        chainl1 pintegerTerm pintegerOp1
    

    // DateExpr parser

    let pdateMeasuredDate : Parser<DateAtom, UserState> = stringReturn "date" DateAtom.MeasuredDate
    let pdateLiteral : Parser<(DateAtom), UserState> =
        pipe3 pint16 (pstring "/" >>. pint16) (pstring "/" >>. pint16) (fun y m d -> DateLiteral(int y, int m, int d))
    let pdateToday : Parser<DateAtom, UserState> = stringReturn "today" Today
    let pdateThisWeek : Parser<DateAtom, UserState> = stringReturn "thisweek" ThisWeek
    let pdateThisMonth : Parser<DateAtom, UserState> = stringReturn "thismonth" ThisMonth

    let pdateAtom : Parser<DateAtom, UserState> =
        (attempt pdateLiteral <|> attempt pdateToday <|> attempt pdateThisWeek <|> attempt pdateThisMonth <|> pdateMeasuredDate) .>> spaces
    
    let pdateExpr, pdateExprImpl = createParserForwardedToRef ()
    let pdateFactor : Parser<DateExpr, UserState> =
        attempt (pbetweenParentheses_ws pdateExpr)
        <|> (pdateAtom |>> DateAtom)
    let pdateTerm : Parser<DateExpr, UserState> =
        attempt (pipe2 pdateFactor (spaces >>. pchar '+' >>. spaces >>. pintegerExpr) (fun x y -> AddDate(x, y)))
        <|> attempt (pipe2 pintegerExpr (spaces >>. pchar '+' >>. spaces >>. pdateFactor) (fun x y -> AddDate(y, x)))
        <|> attempt (pipe2 pdateFactor (spaces >>. pchar '-' >>. spaces >>. pintegerExpr) (fun x y -> ReduceDate(x, y)))
        <|> attempt (pipe2 pintegerExpr (spaces >>. pchar '+' >>. spaces >>. pdateFactor) (fun x y -> ReduceDate(y, x)))
        <|> pdateFactor
    pdateExprImpl :=
        pdateTerm
    
    
    // DecimalExpr parser

    let pdecimalBodyTemperature : Parser<DecimalAtom, UserState> = stringReturn "temp" DecimalAtom.BodyTemperature

    let pdecimalLiteral : Parser<(DecimalAtom), UserState> = pfloat |>> decimal |>> DecimalLiteral

    let pdecimalAtom : Parser<DecimalAtom, UserState> =
        (attempt pdecimalLiteral <|> pdecimalBodyTemperature) .>> spaces

    let pdecimalOp1 =
        choice
            [ pchar '+' .>> spaces >>% (fun x y -> DSum(x, y) )
              pchar '-' .>> spaces >>% (fun x y -> DSub(x, y) ) ]
    let pdecimalOp2 =
        choice
            [ pchar '*' .>> spaces >>% (fun x y -> DMul(x, y) )
              pchar '\\' .>> spaces >>% (fun x y -> DDiv(x, y) ) ]
        
    let pdecimalExpr, pdecimalExprImpl = createParserForwardedToRef ()
    let pdecimalFactor : Parser<DecimalExpr, UserState> =
        attempt (pbetweenParentheses_ws pdecimalExpr)
        <|> (pdecimalAtom |>> DecimalAtom)
    let pdecimalTerm : Parser<DecimalExpr, UserState> =
        chainl1 pdecimalFactor pdecimalOp2
    pdecimalExprImpl :=
        chainl1 pdecimalTerm pdecimalOp1


    // Boolean parser
    let pbooleanExpr, pbooleanExprImpl = createParserForwardedToRef ()

    let pbooleanHasError : Parser<BooleanAtom, UserState> = stringReturn "error" BooleanAtom.HasError
    let pbooleanHasWarning : Parser<BooleanAtom, UserState> = stringReturn "warning" BooleanAtom.HasWarning
    let pbooleanIsSubmitted : Parser<BooleanAtom, UserState> = stringReturn "submitted" BooleanAtom.IsSubmitted
    let pbooleanIsInfected : Parser<BooleanAtom, UserState> = stringReturn "infected" BooleanAtom.IsInfected

    let pbooleanLiteral : Parser<(BooleanAtom), UserState> = 
        attempt (stringReturn "true" BooleanAtom.True) <|> stringReturn "false" BooleanAtom.False
    
    let pbooleanOp1 =
        choice
            [ pstring "&&" .>> spaces >>% (fun x y -> Conj(x, y) )
              pstring "||" .>> spaces >>% (fun x y -> Disj(x, y) )
              pstring "->" .>> spaces >>% (fun x y -> Impl(x, y) ) ]

    let pbooleanOp2 =
        choice
            [ pstring "==" .>> spaces >>% (fun x y -> Eq(x, y) )
              pstring "!=" .>> spaces >>% (fun x y -> Ne(x, y) ) ]

    let pbooleanAtom : Parser<BooleanAtom, UserState> =
        ( attempt pbooleanLiteral .>> spaces
        <|> attempt pbooleanHasError
        <|> attempt pbooleanHasWarning
        <|> attempt pbooleanIsSubmitted
        <|> attempt pbooleanIsInfected
        <|> attempt (pipe2 pstringExpr (spaces >>. pstring "==" >>. spaces >>. pstringExpr) (fun x y -> SEq(x, y)))
        <|> attempt (pipe2 pstringExpr (spaces >>. pstring "!=" >>. spaces >>. pstringExpr) (fun x y -> SNe(x, y)))
        <|> attempt (pipe2 pintegerExpr (spaces >>. pstring "==" >>. spaces >>. pintegerExpr) (fun x y -> IEq(x, y)))
        <|> attempt (pipe2 pintegerExpr (spaces >>. pstring "!=" >>. spaces >>. pintegerExpr) (fun x y -> INe(x, y)))
        <|> attempt (pipe2 pintegerExpr (spaces >>. pstring ">"  >>. spaces >>. pintegerExpr) (fun x y -> IGt(x, y)))
        <|> attempt (pipe2 pintegerExpr (spaces >>. pstring ">=" >>. spaces >>. pintegerExpr) (fun x y -> IGe(x, y)))
        <|> attempt (pipe2 pintegerExpr (spaces >>. pstring "<"  >>. spaces >>. pintegerExpr) (fun x y -> ILt(x, y)))
        <|> attempt (pipe2 pintegerExpr (spaces >>. pstring "<=" >>. spaces >>. pintegerExpr) (fun x y -> ILe(x, y)))
        <|> attempt (pipe2 pdecimalExpr (spaces >>. pstring "==" >>. spaces >>. pdecimalExpr) (fun x y -> DEq(x, y)))
        <|> attempt (pipe2 pdecimalExpr (spaces >>. pstring "!=" >>. spaces >>. pdecimalExpr) (fun x y -> DNe(x, y)))
        <|> attempt (pipe2 pdecimalExpr (spaces >>. pstring ">"  >>. spaces >>. pdecimalExpr) (fun x y -> DGt(x, y)))
        <|> attempt (pipe2 pdecimalExpr (spaces >>. pstring ">=" >>. spaces >>. pdecimalExpr) (fun x y -> DGe(x, y)))
        <|> attempt (pipe2 pdecimalExpr (spaces >>. pstring "<"  >>. spaces >>. pdecimalExpr) (fun x y -> DLt(x, y)))
        <|> attempt (pipe2 pdecimalExpr (spaces >>. pstring "<=" >>. spaces >>. pdecimalExpr) (fun x y -> DLe(x, y)))
        <|> attempt (pipe2 pdateExpr (spaces >>. pstring "==" >>. spaces >>. pdateExpr) (fun x y -> DtEq(x, y)))
        <|> attempt (pipe2 pdateExpr (spaces >>. pstring "!=" >>. spaces >>. pdateExpr) (fun x y -> DtNe(x, y)))
        <|> attempt (pipe2 pdateExpr (spaces >>. pstring ">"  >>. spaces >>. pdateExpr) (fun x y -> DtGt(x, y)))
        <|> attempt (pipe2 pdateExpr (spaces >>. pstring ">=" >>. spaces >>. pdateExpr) (fun x y -> DtGe(x, y)))
        <|> attempt (pipe2 pdateExpr (spaces >>. pstring "<"  >>. spaces >>. pdateExpr) (fun x y -> DtLt(x, y)))
        <|> attempt (pipe2 pdateExpr (spaces >>. pstring "<=" >>. spaces >>. pdateExpr) (fun x y -> DtLe(x, y)))
        ) .>> spaces


    let pbooleanFactor : Parser<BooleanExpr, UserState> =
        attempt (pbetweenParentheses_ws pbooleanExpr)
        <|> (pbooleanAtom |>> BooleanAtom)

    let pbooleanTerm : Parser<BooleanExpr, UserState> = 
        chainl1 pbooleanFactor pbooleanOp2
    pbooleanExprImpl :=
        chainl1 pbooleanTerm pbooleanOp1


    // Expr parser

    pexprImpl :=
        attempt (pbooleanExpr |>> Expr.BooleanExpr)
        <|> attempt (pdateExpr |>> Expr.DateExpr)
        <|> attempt (pintegerExpr |>> Expr.IntegerExpr)
        <|> attempt (pdecimalExpr |>> Expr.DecimalExpr)
        <|> (pstringExpr |>> Expr.StringExpr)


    // Sorting parser

    let pattribute : Parser<SortingAttribute, UserState> =
        attempt (stringReturn "temp" SortingAttribute.BodyTemperature)
        <|> attempt (stringReturn "error" SortingAttribute.HasError)
        <|> attempt (stringReturn "warning" SortingAttribute.HasWarning)
        <|> attempt (stringReturn "infected" SortingAttribute.IsInfected)
        <|> attempt (stringReturn "submitted" SortingAttribute.IsSubmitted)
        <|> attempt (stringReturn "date" SortingAttribute.MeasuredDate)
        <|> attempt (stringReturn "timeframe" SortingAttribute.TimeFrame)
        <|> (stringReturn "user" SortingAttribute.UserId)
    let pordering : Parser<Ordering, UserState> =
        (
        attempt (pchar '~' >>. pattribute |>> (fun x -> (x, Desc)))
        <|> (pattribute |>> (fun x -> (x, Asc)))
        ) .>> spaces


    // Query Parser

    let pcondition : Parser<BooleanExpr list, UserState> =
        sepBy pbooleanExpr (pchar ',' .>> spaces)

    let porderings : Parser<Ordering list, UserState> =
        sepBy pordering (pchar ',' .>> spaces)

    let pquery : Parser<Query, UserState> =
        (
        attempt (pipe2 pcondition (spaces >>. pstring "order by" >>. spaces >>. porderings) (fun x y -> (x, Some(y))))
        <|> (pcondition |>> (fun x -> (x, None)))
        ) .>> eof

    let ParseQuery query = 
        match run pquery query with
        | Success (result, _, _) -> Some(result)
        | Failure (errmsg, _, _) -> None