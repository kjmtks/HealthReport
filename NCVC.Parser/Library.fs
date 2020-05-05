namespace NCVC.Parser

open FParsec

module QueryParser =

    type Reference =
        | HasError
        | HasWarning
        | IsSubmitted
        | IsInfected
        | MeasuredDate
        | BodyTemperature
        | UserId
        | TimeFrame
        | Tag

    type StringAtom =
        | UserId
        | TimeFrame
        | Tag
        | StringLiteral of string
    and StringExpr =
        | StringAtom of StringAtom
    
    type DecimalAtom =
        | BodyTemperature
        | DecimalLiteral of decimal
    and DecimalExpr =
        | DecimalAtom of DecimalAtom
        | Minus of DecimalExpr
        | Sum of DecimalExpr * DecimalExpr
        | Sub of DecimalExpr * DecimalExpr
        | Mul of DecimalExpr * DecimalExpr
        | Div of DecimalExpr * DecimalExpr

    type DateTimeSpan =
        | Day
        | Month

    type DateAtom =
        | MeasuredDate
        | DateLiteral of int * int * int
        | Today
        | ThisWeek
        | ThisMonth
    and DateExpr =
        | DateAtom of DateAtom
        | AddDate of DateExpr * DecimalExpr
        | ReduceDate of DateExpr * DecimalExpr

    type BooleanAtom =
        | HasError
        | HasWarning
        | IsSubmitted
        | IsInfected
        | True
        | False
        | SEq of StringExpr * StringExpr
        | SNe of StringExpr * StringExpr
        | SStartWith of StringExpr * StringExpr
        | SEndWith of StringExpr * StringExpr
        | SHas of StringExpr * StringExpr
        | DeEq of DecimalExpr * DecimalExpr
        | DeNe of DecimalExpr * DecimalExpr
        | DeGt of DecimalExpr * DecimalExpr
        | DeGe of DecimalExpr * DecimalExpr
        | DeLt of DecimalExpr * DecimalExpr
        | DeLe of DecimalExpr * DecimalExpr
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

    type SortingAttribute =
        | UserId
        | TimeFrame
        | MeasuredDate
        | BodyTemperature
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


    // String parser

    let pstringUserId : Parser<StringAtom, UserState> = stringReturn "user" StringAtom.UserId
    let pstringTimeFrame : Parser<StringAtom, UserState> = stringReturn "timeframe" StringAtom.TimeFrame
    let pstringHasTag : Parser<StringAtom, UserState> = stringReturn "tag" StringAtom.Tag

    let pstringLiteral : Parser<(StringAtom), UserState> =
        manyChars (noneOf ['"']) |> between (pchar '"') (pchar '"') .>> spaces |>> string |>> StringLiteral
    let pstringAtom = 
        (attempt pstringUserId <|> attempt pstringTimeFrame <|> attempt pstringLiteral <|> pstringHasTag) .>> spaces
    let pstringExpr = pstringAtom |>> StringAtom
    
    // DecimalExpr parser

    let pdecimalBodyTemperature : Parser<DecimalAtom, UserState> = stringReturn "temp" DecimalAtom.BodyTemperature

    let pdecimalLiteral : Parser<(DecimalAtom), UserState> = pfloat |>> decimal |>> DecimalLiteral

    let pdecimalAtom : Parser<DecimalAtom, UserState> =
        (attempt pdecimalLiteral <|> pdecimalBodyTemperature) .>> spaces

    let pdecimalOp1 =
        choice
            [ pchar '+' .>> spaces >>% (fun x y -> Sum(x, y) )
              pchar '-' .>> spaces >>% (fun x y -> Sub(x, y) ) ]
    let pdecimalOp2 =
        choice
            [ pchar '*' .>> spaces >>% (fun x y -> Mul(x, y) )
              pchar '\\' .>> spaces >>% (fun x y -> Div(y, x) ) ]
        
    let pdecimalExpr, pdecimalExprImpl = createParserForwardedToRef ()
    let pdecimalFactor : Parser<DecimalExpr, UserState> =
        attempt (pbetweenParentheses_ws pdecimalExpr)
        <|> (pstring "-" >>. spaces >>. pdecimalExpr |>> Minus)
        <|> (pdecimalAtom |>> DecimalAtom)
    let pdecimalTerm : Parser<DecimalExpr, UserState> =
        chainl1 pdecimalFactor pdecimalOp2
    pdecimalExprImpl :=
        chainl1 pdecimalTerm pdecimalOp1

        
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
        attempt (pipe2 pdateFactor (spaces >>. pchar '+' >>. spaces >>. pdecimalExpr) (fun x y -> AddDate(x, y)))
        <|> attempt (pipe2 pdecimalExpr (spaces >>. pchar '+' >>. spaces >>. pdateFactor) (fun x y -> AddDate(y, x)))
        <|> attempt (pipe2 pdateFactor (spaces >>. pchar '-' >>. spaces >>. pdecimalExpr) (fun x y -> ReduceDate(x, y)))
        <|> attempt (pipe2 pdecimalExpr (spaces >>. pchar '+' >>. spaces >>. pdateFactor) (fun x y -> ReduceDate(y, x)))
        <|> pdateFactor
    pdateExprImpl :=
        pdateTerm
    

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
        <|> attempt (pipe2 pstringExpr (spaces >>. pstring "=*" >>. spaces >>. pstringExpr) (fun x y -> SStartWith(x, y)))
        <|> attempt (pipe2 pstringExpr (spaces >>. pstring "*=" >>. spaces >>. pstringExpr) (fun x y -> SEndWith(x, y)))
        <|> attempt (pipe2 pstringExpr (spaces >>. pstring "->" >>. spaces >>. pstringExpr) (fun x y -> SHas(x, y)))
        <|> attempt (pipe2 pdecimalExpr (spaces >>. pstring "==" >>. spaces >>. pdecimalExpr) (fun x y -> DeEq(x, y)))
        <|> attempt (pipe2 pdecimalExpr (spaces >>. pstring "!=" >>. spaces >>. pdecimalExpr) (fun x y -> DeNe(x, y)))
        <|> attempt (pipe2 pdecimalExpr (spaces >>. pstring ">"  >>. spaces >>. pdecimalExpr) (fun x y -> DeGt(x, y)))
        <|> attempt (pipe2 pdecimalExpr (spaces >>. pstring ">=" >>. spaces >>. pdecimalExpr) (fun x y -> DeGe(x, y)))
        <|> attempt (pipe2 pdecimalExpr (spaces >>. pstring "<"  >>. spaces >>. pdecimalExpr) (fun x y -> DeLt(x, y)))
        <|> attempt (pipe2 pdecimalExpr (spaces >>. pstring "<=" >>. spaces >>. pdecimalExpr) (fun x y -> DeLe(x, y)))
        <|> attempt (pipe2 pdateExpr (spaces >>. pstring "==" >>. spaces >>. pdateExpr) (fun x y -> DtEq(x, y)))
        <|> attempt (pipe2 pdateExpr (spaces >>. pstring "!=" >>. spaces >>. pdateExpr) (fun x y -> DtNe(x, y)))
        <|> attempt (pipe2 pdateExpr (spaces >>. pstring ">"  >>. spaces >>. pdateExpr) (fun x y -> DtGt(x, y)))
        <|> attempt (pipe2 pdateExpr (spaces >>. pstring ">=" >>. spaces >>. pdateExpr) (fun x y -> DtGe(x, y)))
        <|> attempt (pipe2 pdateExpr (spaces >>. pstring "<"  >>. spaces >>. pdateExpr) (fun x y -> DtLt(x, y)))
        <|> attempt (pipe2 pdateExpr (spaces >>. pstring "<=" >>. spaces >>. pdateExpr) (fun x y -> DtLe(x, y)))
        ) .>> spaces


    let pbooleanFactor : Parser<BooleanExpr, UserState> =
        attempt (pbetweenParentheses_ws pbooleanExpr)
        <|> (pstring "!" >>. spaces >>. pbooleanExpr |>> Neg)
        <|> (pbooleanAtom |>> BooleanAtom)

    let pbooleanTerm : Parser<BooleanExpr, UserState> = 
        chainl1 pbooleanFactor pbooleanOp2
    pbooleanExprImpl :=
        chainl1 pbooleanTerm pbooleanOp1



    // Sorting parser

    let pattribute : Parser<SortingAttribute, UserState> =
        attempt (stringReturn "temp" SortingAttribute.BodyTemperature)
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
        spaces >>. 
            (
            attempt (pipe2 pcondition (spaces >>. pstring "order by" >>. spaces >>. porderings) (fun x y -> (x, Some(y))))
            <|> (pcondition |>> (fun x -> (x, None)))
            ) .>> eof


    // Functions

    let ParseQuery queryString = 
        match run pquery queryString with
        | Success (result, _, _) -> Some(result)
        | Failure (_, _, _) -> None
        
    let HasError queryString = 
        match run pquery queryString with
        | Success (_, _, _) ->  None
        | Failure (errmsg, _, _) -> Some(errmsg)




    let BooleanExprToPgsqlExprString (expr : BooleanExpr) : string =
        let rec _booleanAtom (atom : BooleanAtom) =
            match atom with
            | BooleanAtom.True -> "TRUE"
            | BooleanAtom.False -> "FALSE"
            | BooleanAtom.IsInfected -> @"h.""IsInfected"""
            | BooleanAtom.IsSubmitted -> @"(NOT h.""IsEmptyData"")"
            | BooleanAtom.HasError -> @"h.""HasError"""
            | BooleanAtom.HasWarning -> @"h.""HasWarning"""
            | BooleanAtom.SEq (lhs, rhs) -> sprintf "(%s = %s)" (_stringExpr lhs) (_stringExpr rhs)
            | BooleanAtom.SNe (lhs, rhs) -> sprintf "(%s <> %s)" (_stringExpr lhs) (_stringExpr rhs)
            | BooleanAtom.SStartWith (lhs, rhs) ->
                sprintf "(%s LIKE '%s%%' AND %s IS NOT NULL)"
                    (_stringExpr lhs)
                    ((_stringExpr rhs).Trim('\'').Replace("'", "''").Replace("_", "\\_").Replace("%", "\\%"))
                    (_stringExpr lhs)
            | BooleanAtom.SEndWith (lhs, rhs) ->
                sprintf "(%s LIKE '%%%s' AND %s IS NOT NULL)"
                    (_stringExpr lhs)
                    ((_stringExpr rhs).Trim('\'').Replace("'", "''").Replace("_", "\\_").Replace("%", "\\%"))
                    (_stringExpr lhs)
            | BooleanAtom.SHas (lhs, rhs) ->
                sprintf "(%s LIKE '%% %s %%' AND %s IS NOT NULL)"
                    (_stringExpr lhs)
                    ((_stringExpr rhs).Trim('\'').Replace("'", "''").Replace("_", "\\_").Replace("%", "\\%"))
                    (_stringExpr lhs)
            | BooleanAtom.DeEq (lhs, rhs) -> sprintf "(%s = %s)" (_decimalExpr lhs) (_decimalExpr rhs)
            | BooleanAtom.DeNe (lhs, rhs) -> sprintf "(%s <> %s)" (_decimalExpr lhs) (_decimalExpr rhs)
            | BooleanAtom.DeGt (lhs, rhs) -> sprintf "(%s > %s)" (_decimalExpr lhs) (_decimalExpr rhs)
            | BooleanAtom.DeGe (lhs, rhs) -> sprintf "(%s >= %s)" (_decimalExpr lhs) (_decimalExpr rhs)
            | BooleanAtom.DeLt (lhs, rhs) -> sprintf "(%s < %s)" (_decimalExpr lhs) (_decimalExpr rhs)
            | BooleanAtom.DeLe (lhs, rhs) -> sprintf "(%s <= %s)" (_decimalExpr lhs) (_decimalExpr rhs)
            | BooleanAtom.DtEq (lhs, rhs) -> sprintf "(%s = %s)" (_dateExpr lhs |> fst) (_dateExpr rhs |> fst)
            | BooleanAtom.DtNe (lhs, rhs) -> sprintf "(%s <> %s)" (_dateExpr lhs |> fst) (_dateExpr rhs |> fst)
            | BooleanAtom.DtGt (lhs, rhs) -> sprintf "(%s > %s)" (_dateExpr lhs |> fst) (_dateExpr rhs |> fst)
            | BooleanAtom.DtGe (lhs, rhs) -> sprintf "(%s >= %s)" (_dateExpr lhs |> fst) (_dateExpr rhs |> fst)
            | BooleanAtom.DtLt (lhs, rhs) -> sprintf "(%s < %s)" (_dateExpr lhs |> fst) (_dateExpr rhs |> fst)
            | BooleanAtom.DtLe (lhs, rhs) -> sprintf "(%s <= %s)" (_dateExpr lhs |> fst) (_dateExpr rhs |> fst)
        and _stringAtom (atom : StringAtom) : string =
            match atom with
            | StringAtom.UserId -> @"h.""Account"""
            | StringAtom.Tag -> @"(' ' || h.""Tags"" || ' ')"
            | StringAtom.TimeFrame -> @"h.""TimeFrame"""
            | StringAtom.StringLiteral literal -> sprintf "'%s'" (literal.Replace("'", "''"))
        and _decimalAtom (atom : DecimalAtom) : string =
            match atom with
            | DecimalAtom.BodyTemperature -> @"h.""BodyTemperature"""
            | DecimalAtom.DecimalLiteral literal -> literal.ToString()
        and _dateAtom (atom : DateAtom) : (string * DateTimeSpan) =
            match atom with
            | DateAtom.MeasuredDate -> (@"h.""MeasuredAt""", DateTimeSpan.Day)
            | DateAtom.DateLiteral (y, m, d) -> (sprintf "(date '%04d-%02d-%02d')" y m d, DateTimeSpan.Day)
            | DateAtom.Today ->
                let today = System.DateTime.Today;
                in (sprintf "(date '%04d-%02d-%02d')" today.Year today.Month today.Day, DateTimeSpan.Day)
            | DateAtom.ThisWeek ->
                let today = System.DateTime.Today
                let diff = (7 + ((int today.DayOfWeek) - (int System.DayOfWeek.Monday))) % 7
                let start = today.AddDays(float -diff).Date
                in (sprintf "(date '%04d-%02d-%02d')" start.Year start.Month start.Day, DateTimeSpan.Day)
            | DateAtom.ThisMonth ->
                let today = System.DateTime.Today;
                in (sprintf "(date '%04d-%02d-%02d')" today.Year today.Month 1, DateTimeSpan.Month)
        and _booleanExpr (expr : BooleanExpr) : string =
            match expr with
            | BooleanAtom atom -> _booleanAtom atom
            | Eq (lhs, rhs) -> sprintf "(%s = %s)" (_booleanExpr lhs) (_booleanExpr rhs)
            | Ne (lhs, rhs) -> sprintf "(%s <> %s)" (_booleanExpr lhs) (_booleanExpr rhs)
            | Neg inner -> sprintf "(NOT %s)" (_booleanExpr inner)
            | Conj (lhs, rhs) -> sprintf "(%s AND %s)" (_booleanExpr lhs) (_booleanExpr rhs)
            | Disj (lhs, rhs) -> sprintf "(%s OR %s)" (_booleanExpr lhs) (_booleanExpr rhs)
            | Impl (lhs, rhs) -> sprintf "((NOT %s) OR %s)" (_booleanExpr lhs) (_booleanExpr rhs)
        and _stringExpr (expr : StringExpr) : string =
            match expr with
            | StringAtom atom -> _stringAtom atom
        and _decimalExpr (expr : DecimalExpr) : string =
            match expr with
            | DecimalAtom atom -> _decimalAtom atom
            | Sum (lhs, rhs) -> sprintf "(%s + %s)" (_decimalExpr lhs) (_decimalExpr rhs)
            | Sub (lhs, rhs) -> sprintf "(%s - %s)" (_decimalExpr lhs) (_decimalExpr rhs)
            | Mul (lhs, rhs) -> sprintf "(%s * %s)" (_decimalExpr lhs) (_decimalExpr rhs)
            | Div (lhs, rhs) -> sprintf "(%s / %s)" (_decimalExpr lhs) (_decimalExpr rhs)
            | Minus inner -> sprintf "(-%s)" (_decimalExpr inner)
        and _dateExpr (expr : DateExpr) : (string * DateTimeSpan) =
            match expr with
            | DateAtom atom -> _dateAtom atom
            | AddDate (lhs, rhs) ->
                let date, span = _dateExpr lhs
                match span with
                | DateTimeSpan.Day ->  (sprintf "(%s + (interval '%s day'))" date (_decimalExpr rhs), DateTimeSpan.Day)
                | DateTimeSpan.Month ->  (sprintf "(%s + (interval '%s month'))" date (_decimalExpr rhs), DateTimeSpan.Month)
            | ReduceDate (lhs, rhs) ->
                let date, span = _dateExpr lhs
                match span with
                | DateTimeSpan.Day ->  (sprintf "(%s - (interval '%s day'))" date (_decimalExpr rhs), DateTimeSpan.Day)
                | DateTimeSpan.Month ->  (sprintf "(%s - (interval '%s month'))" date (_decimalExpr rhs), DateTimeSpan.Month)
        _booleanExpr expr

    let GetReferences (expr : BooleanExpr) : Reference list =
        let rec _booleanAtom (atom : BooleanAtom) (xs : Reference list) =
            match atom with
            | BooleanAtom.IsInfected -> Reference.IsInfected::xs
            | BooleanAtom.IsSubmitted -> Reference.IsSubmitted::xs
            | BooleanAtom.HasError -> Reference.HasError::xs
            | BooleanAtom.HasWarning -> Reference.HasWarning::xs
            | BooleanAtom.SEq (lhs, rhs) -> (_stringExpr lhs xs) @ (_stringExpr rhs xs) @ xs
            | BooleanAtom.SNe (lhs, rhs) -> (_stringExpr lhs xs) @ (_stringExpr rhs xs) @ xs
            | BooleanAtom.SStartWith (lhs, rhs) -> (_stringExpr lhs xs) @ (_stringExpr rhs xs) @ xs
            | BooleanAtom.SEndWith (lhs, rhs) -> (_stringExpr lhs xs) @ (_stringExpr rhs xs) @ xs
            | BooleanAtom.SHas (lhs, rhs) -> (_stringExpr lhs xs) @ (_stringExpr rhs xs) @ xs
            | BooleanAtom.DeEq (lhs, rhs) -> (_decimalExpr lhs xs) @ (_decimalExpr rhs xs) @ xs
            | BooleanAtom.DeNe (lhs, rhs) -> (_decimalExpr lhs xs) @ (_decimalExpr rhs xs) @ xs
            | BooleanAtom.DeGt (lhs, rhs) -> (_decimalExpr lhs xs) @ (_decimalExpr rhs xs) @ xs
            | BooleanAtom.DeGe (lhs, rhs) -> (_decimalExpr lhs xs) @ (_decimalExpr rhs xs) @ xs
            | BooleanAtom.DeLt (lhs, rhs) -> (_decimalExpr lhs xs) @ (_decimalExpr rhs xs) @ xs
            | BooleanAtom.DeLe (lhs, rhs) -> (_decimalExpr lhs xs) @ (_decimalExpr rhs xs) @ xs
            | BooleanAtom.DtEq (lhs, rhs) -> (_dateExpr lhs xs) @ (_dateExpr rhs xs) @ xs
            | BooleanAtom.DtNe (lhs, rhs) -> (_dateExpr lhs xs) @ (_dateExpr rhs xs) @ xs
            | BooleanAtom.DtGt (lhs, rhs) -> (_dateExpr lhs xs) @ (_dateExpr rhs xs) @ xs
            | BooleanAtom.DtGe (lhs, rhs) -> (_dateExpr lhs xs) @ (_dateExpr rhs xs) @ xs
            | BooleanAtom.DtLt (lhs, rhs) -> (_dateExpr lhs xs) @ (_dateExpr rhs xs) @ xs
            | BooleanAtom.DtLe (lhs, rhs) -> (_dateExpr lhs xs) @ (_dateExpr rhs xs) @ xs
            | _ -> xs
        and _stringAtom (atom : StringAtom) (xs : Reference list) : Reference list =
            match atom with
            | StringAtom.UserId -> Reference.UserId::xs
            | StringAtom.Tag -> Reference.Tag::xs
            | StringAtom.TimeFrame -> Reference.TimeFrame::xs
            | _ -> xs
        and _decimalAtom (atom : DecimalAtom) (xs : Reference list) : Reference list =
            match atom with
            | DecimalAtom.BodyTemperature -> Reference.BodyTemperature::xs
            | _ -> xs
        and _dateAtom (atom : DateAtom) (xs : Reference list) : Reference list=
            match atom with
            | DateAtom.MeasuredDate -> Reference.MeasuredDate::xs
            | _ -> xs
        and _booleanExpr (expr : BooleanExpr) (xs : Reference list) : Reference list =
            match expr with
            | BooleanAtom atom -> (_booleanAtom atom xs) @ xs
            | Eq (lhs, rhs) -> (_booleanExpr lhs xs) @ (_booleanExpr rhs xs) @ xs
            | Ne (lhs, rhs) -> (_booleanExpr lhs xs) @ (_booleanExpr rhs xs) @ xs
            | Neg inner -> (_booleanExpr inner xs) @ xs
            | Conj (lhs, rhs) -> (_booleanExpr lhs xs) @ (_booleanExpr rhs xs) @ xs
            | Disj (lhs, rhs) -> (_booleanExpr lhs xs) @ (_booleanExpr rhs xs) @ xs
            | Impl (lhs, rhs) -> (_booleanExpr lhs xs) @ (_booleanExpr rhs xs) @ xs
        and _stringExpr (expr : StringExpr) (xs : Reference list) : Reference list =
            match expr with
            | StringAtom atom -> (_stringAtom atom xs) @ xs
        and _decimalExpr (expr : DecimalExpr) (xs : Reference list) : Reference list =
            match expr with
            | DecimalAtom atom -> (_decimalAtom atom xs) @ xs
            | Sum (lhs, rhs) -> (_decimalExpr lhs xs) @ (_decimalExpr rhs xs) @ xs
            | Sub (lhs, rhs) -> (_decimalExpr lhs xs) @ (_decimalExpr rhs xs) @ xs
            | Mul (lhs, rhs) -> (_decimalExpr lhs xs) @ (_decimalExpr rhs xs) @ xs
            | Div (lhs, rhs) -> (_decimalExpr lhs xs) @ (_decimalExpr rhs xs) @ xs
            | Minus inner -> _decimalExpr inner xs
        and _dateExpr (expr : DateExpr) (xs : Reference list) : Reference list =
            match expr with
            | DateAtom atom -> (_dateAtom atom xs) @ xs
            | AddDate (lhs, rhs) -> (_dateExpr lhs xs) @ (_decimalExpr rhs xs) @ xs
            | ReduceDate (lhs, rhs) -> (_dateExpr lhs xs) @ (_decimalExpr rhs xs) @ xs
        _booleanExpr expr []