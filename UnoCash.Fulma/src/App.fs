module App.View

open Elmish
open Fable.React
open Fable.React.Props
open Fulma
open Fable.FontAwesome
open System
open Elmish.Debug
open Elmish.HMR

type Tab =
    | AddExpense
    | ShowExpenses
    | ShowStatistics
    | About

type AlertType =
    | None
    | DuplicateTag

type Model =
    {
        CurrentTab : Tab
        Amount : decimal
        Tags : string list
        TagsText : string
        Alert : AlertType
        Date : DateTime
    }

type Msg =
    | ChangeToTab of Tab
    | ChangeAmount of string
    | TagsKeyDown of string * string
    | TagsTextChanged of string
    | TagDelete of string
    | DateChanged of string

let init _ =
    {
        CurrentTab = AddExpense
        Amount = 0m
        Tags = [ ]
        TagsText = ""
        Alert = None
        Date = DateTime.Today
    }, Cmd.none

let private sanitize value =
    match Decimal.TryParse(value) with
    | true, dec -> (dec * 100m |> Decimal.Truncate) / 100m
    | false, _  -> 0m

let private update msg model =
    match msg with
    | ChangeToTab newTab    -> { model with CurrentTab = newTab }, Cmd.none
    | ChangeAmount newValue -> { model with Amount = sanitize newValue }, Cmd.none
    | TagsKeyDown (key, x)  -> match key with
                               | "Enter" -> {
                                                model with Tags = x :: model.Tags |> List.distinct
                                                           TagsText = String.Empty
                                                           Alert = match model.Tags |> List.exists ((=)x) with
                                                                   | true  -> DuplicateTag
                                                                   | false -> None
                                            }, Cmd.none
                               | _       -> model, Cmd.none
    | TagsTextChanged text  -> { model with TagsText = text }, Cmd.none
    | TagDelete tagName     -> { model with Tags = model.Tags |> List.except [ tagName ] }, Cmd.none
    | DateChanged newDate   -> { model with Date = DateTime.Parse(newDate) }, Cmd.none

let private tab model dispatch tabType title =
    Tabs.tab [ Tabs.Tab.IsActive (model.CurrentTab = tabType) ]
             [ a [ OnClick (fun _ -> ChangeToTab tabType |> dispatch) ] [ str title ] ]

let private receiptUpload =
    Field.div [ ]
              [ File.file [ File.HasName ]
                          [ File.label [ ]
                                       [ File.input [ ]
                                         File.cta [ ]
                                                  [ File.icon [ ]
                                                              [ Icon.icon [ ]
                                                                          [ Fa.i [ Fa.Solid.Upload ] [ ] ] ]
                                                    File.label [ ] [ str "Upload a receipt..." ] ]
                                         File.name [ ] [ str "No receipt selected" ] ] ] ]

let private dropdown title items =
    let options items =
        items |>
        List.map (fun item -> option [ Value item ] [ str item ])

    Field.div [ ]
              [ Label.label [ ] [ str title ]
                Control.div [ ]
                            [ Select.select [ ]
                                            [ select [ DefaultValue (items |> List.head) ]
                                                     (options items) ] ] ]

let private tags model dispatch =
    let tag name =
        let iconLookup name =
            match name with
            | "groceries" -> Fa.Solid.ShoppingBag
            | "fuel"      -> Fa.Solid.Car
            | _           -> Fa.Solid.Tag
        Control.div [ ]
                    [ Tag.list [ Tag.List.HasAddons ]
                               [ Tag.tag [ Tag.Color IsInfo ] [ Icon.icon [ ] [ Fa.i [ iconLookup name ] [ ] ] ]
                                 Tag.tag [ Tag.Color IsLight ] [ str name ]
                                 Tag.delete [ Tag.Props [ OnClick (fun _ -> TagDelete name |> dispatch) ] ] [ ] ] ]

    model.Tags |>
    List.map tag |>
    Field.div [ Field.IsGroupedMultiline ]

let private ifDuplicateTagAlertAdd element model =
    match model.Alert with
    | DuplicateTag -> [ element ]
    | _            -> []

let private addExpensePage model dispatch =
    form [ ]
         [ receiptUpload
                    
           Field.div [ ]
                     [ Label.label [ ] [ str "Payee" ]
                       Control.div [ Control.HasIconLeft ]
                                   [ Input.text [ Input.Placeholder "Ex: Tesco"
                                                  Input.Props [ AutoFocus true ] ]
                                     Icon.icon [ Icon.Size IsSmall; Icon.IsLeft ]
                                               [ Fa.i [ Fa.Solid.CashRegister ] [ ] ] ] ]

           Field.div [ ]
                [ Label.label [ ] [ str "Date" ]
                  Control.div [ Control.HasIconLeft ]
                              [ Input.date [ Input.Value (model.Date.ToString("yyyy-MM-dd"))
                                             Input.OnChange (fun ev -> DateChanged ev.Value |> dispatch) ]
                                Icon.icon [ Icon.Size IsSmall; Icon.IsLeft ]
                                          [ Fa.i [ Fa.Solid.CalendarDay ] [ ] ] ] ]

           Field.div [ ]
                     [ Label.label [ ] [ str "Amount" ]
                       Control.div [ Control.HasIconLeft ]
                                   [ Input.number [ Input.Props [ Props.Step "0.01" ]
                                                    Input.OnChange (fun ev -> ChangeAmount ev.Value |> dispatch)
                                                    Input.Value (string model.Amount) ]
                                     Icon.icon [ Icon.Size IsSmall
                                                 Icon.IsLeft ]
                                               [ Fa.i [ Fa.Solid.DollarSign ] [ ] ] ] ]
 
           div [ Style [ Display DisplayOptions.InlineFlex ] ]
               [ dropdown "Account" [ "Current"; "ISA"; "Wallet" ]

                 dropdown "Status" [ "New"; "Pending"; "Reconciled" ]

                 dropdown "Type" [ "Regular"; "Internal transfer"; "Scheduled" ] ]

           Field.div [ ]
                     ([ Label.label [ ] [ str "Tags" ]
                        Control.div [ Control.HasIconLeft; Control.HasIconRight ]
                                    ([ Input.text ([ Input.Placeholder "Ex: groceries"
                                                     Input.Value model.TagsText
                                                     Input.OnChange (fun ev -> TagsTextChanged ev.Value |> dispatch)
                                                     Input.Props [ OnKeyDown (fun ev -> TagsKeyDown (ev.key, ev.Value) |> dispatch) ] ] @
                                                     ifDuplicateTagAlertAdd (Input.Color IsDanger) model)
                                       Icon.icon [ Icon.Size IsSmall; Icon.IsLeft ] [ Fa.i [ Fa.Solid.Tags ] [ ] ] ] @
                                       (ifDuplicateTagAlertAdd (Icon.icon [ Icon.Size IsSmall; Icon.IsRight ] [ Fa.i [ Fa.Solid.ExclamationTriangle ] [ ] ]) model)) ] @ 
                        (ifDuplicateTagAlertAdd (Help.help [ Help.Color IsDanger ] [ str "Duplicate tag" ]) model))
           
           tags model dispatch

           Field.div [ ]
                     [ Label.label [ ] [ str "Description" ]
                       Control.div [ Control.IsLoading true ]
                                   [ Textarea.textarea [ ] [ ] ] ] ]

let private page model dispatch =
    match model.CurrentTab with
    | AddExpense   -> Card.card [ ]
                                [ Card.content [ ]
                                               [ Content.content [ ]
                                                                 [ addExpensePage model dispatch ] ]
                                  Card.footer [ ]
                                              [ Card.Footer.a [ ]
                                                              [ str "Add" ]
                                                Card.Footer.a [ ]
                                                              [ str "Split" ] ] ]
    | _ -> div [ ] [ str "Not implemented" ]

let private view model dispatch =
    div [ ] [ Tabs.tabs [ Tabs.IsCentered ]
                        [ tab model dispatch AddExpense     "Add expense"
                          tab model dispatch ShowExpenses   "Show expenses" 
                          tab model dispatch ShowStatistics "Statistics"    
                          tab model dispatch About          "About"         ]
              page model dispatch ]

Program.mkProgram init update view
|> Program.withReactSynchronous "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run