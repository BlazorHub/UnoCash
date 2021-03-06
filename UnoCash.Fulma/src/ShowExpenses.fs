module UnoCash.Fulma.ShowExpenses

open Fable.React
open Fable.React.Props
open Fulma
open Fable.FontAwesome
open UnoCash.Fulma.Models
open UnoCash.Fulma.Messages
open UnoCash.Fulma.Helpers
open Feliz

let private expensesTable model dispatch =
    let expensesRows =
        let row (expense : Expense) =
            let cellButton message icon =
                a [ onClick message dispatch
                    Style [ PaddingLeft "10px"; PaddingRight "10px" ] ]
                  [ Fa.i [ icon ] [] ]
                  
            let deleteButton =
                cellButton (DeleteExpense expense.id) Fa.Solid.Trash
            
            let editButton =
                cellButton (EditExpense expense) Fa.Solid.PencilAlt
            
            let cells =
                [
                    expense.date
                    expense.payee
                    expense.amount |> string
                    expense.status
                    expense.``type``
                    expense.tags
                    expense.description
                ] |>
                List.map (fun cellContent -> td [] [ str cellContent ])
            
            let cellsWithActions =
                cells @ [ td [ Style [ WhiteSpace WhiteSpaceOptions.Nowrap ] ]
                             [ deleteButton
                               editButton ] ]
            
            tr [] cellsWithActions
               
        model.Expenses |>
        Array.map row
    
    let columns =
        [
            "Date"
            "Payee"
            "Amount"
            "Status"
            "Type"
            "Tags"
            "Description"
        ] |>
        List.map (fun columnName -> th [] [ str columnName ])
    
    let columnsWithActions =
        columns @ [ th [ Style [ Width "1%" ] ] [ str "Actions" ] ]
    
    let tableHeader =
        thead []
              [ tr []
                   columnsWithActions ]
    
    let tableBody =
        tbody [] expensesRows
    
    let table =
        Table.table [ Table.IsBordered
                      Table.IsFullWidth
                      Table.IsStriped ]
                    [ tableHeader
                      tableBody ]
    
    let spinnerIcon =
        Fa.i [ Fa.Solid.Sync; Fa.Spin ] []
    
    let spinner =
        div [ Class ("block " + Fa.Classes.Size.Fa3x)
              Style [ TextAlign TextAlignOptions.Center ] ]
            [ spinnerIcon ]
    
    match model.ExpensesLoaded with
    | true  -> table
    | false -> spinner

let showExpensesCard model dispatch =
    card [ dropdownWithEvent "Account" model.Accounts model.ShowAccount ChangeShowAccount dispatch
           expensesTable model dispatch ]
         Html.none