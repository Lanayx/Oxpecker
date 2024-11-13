namespace Oxpecker.Solid

open Fable.Core

module Aria =

    type HtmlTag with
        // aria role
        [<Erase>]
        member this.role
            with set (value: string) = ()
        // aria attributes
        [<Erase>]
        member this.ariaActiveDescendant
            with set (value: string) = ()
        [<Erase>]
        member this.ariaAtomic
            with set (value: bool) = ()
        [<Erase>]
        member this.ariaAutoComplete
            with set (value: string) = ()
        [<Erase>]
        member this.ariaBrailleLabel
            with set (value: string) = ()
        [<Erase>]
        member this.ariaBrailleRoleDescription
            with set (value: string) = ()
        [<Erase>]
        member this.ariaBusy
            with set (value: bool) = ()
        [<Erase>]
        member this.ariaChecked
            with set (value: string) = ()
        [<Erase>]
        member this.ariaColCount
            with set (value: int) = ()
        [<Erase>]
        member this.ariaColIndex
            with set (value: int) = ()
        [<Erase>]
        member this.ariaColIndexText
            with set (value: string) = ()
        [<Erase>]
        member this.ariaControls
            with set (value: string) = ()
        [<Erase>]
        member this.ariaCurrent
            with set (value: string) = ()
        [<Erase>]
        member this.ariaDescribedBy
            with set (value: string) = ()
        [<Erase>]
        member this.ariaDescription
            with set (value: string) = ()
        [<Erase>]
        member this.ariaDetails
            with set (value: string) = ()
        [<Erase>]
        member this.ariaDisabled
            with set (value: bool) = ()
        [<Erase>]
        member this.ariaErrorMessage
            with set (value: string) = ()
        [<Erase>]
        member this.ariaExpanded
            with set (value: bool) = ()
        [<Erase>]
        member this.ariaFlowTo
            with set (value: string) = ()
        [<Erase>]
        member this.ariaHasPopup
            with set (value: string) = ()
        [<Erase>]
        member this.ariaHidden
            with set (value: bool) = ()
        [<Erase>]
        member this.ariaInvalid
            with set (value: string) = ()
        [<Erase>]
        member this.ariaKeyShortcuts
            with set (value: string) = ()
        [<Erase>]
        member this.ariaLabel
            with set (value: string) = ()
        [<Erase>]
        member this.ariaLabelledBy
            with set (value: string) = ()
        [<Erase>]
        member this.ariaLevel
            with set (value: int) = ()
        [<Erase>]
        member this.ariaLive
            with set (value: string) = ()
        [<Erase>]
        member this.ariaModal
            with set (bool: bool) = ()
        [<Erase>]
        member this.ariaMultiLine
            with set (bool: bool) = ()
        [<Erase>]
        member this.ariaMultiSelectable
            with set (bool: bool) = ()
        [<Erase>]
        member this.ariaOrientation
            with set (value: string) = ()
        [<Erase>]
        member this.ariaOwns
            with set (value: string) = ()
        [<Erase>]
        member this.ariaPlaceholder
            with set (value: string) = ()
        [<Erase>]
        member this.ariaPosInSet
            with set (value: int) = ()
        [<Erase>]
        member this.ariaPressed
            with set (value: string) = ()
        [<Erase>]
        member this.ariaReadOnly
            with set (value: string) = ()
        [<Erase>]
        member this.ariaRelevant
            with set (value: string) = ()
        [<Erase>]
        member this.ariaRequired
            with set (bool: bool) = ()
        [<Erase>]
        member this.ariaRoleDescription
            with set (value: string) = ()
        [<Erase>]
        member this.ariaRowCount
            with set (value: int) = ()
        [<Erase>]
        member this.ariaRowIndex
            with set (value: int) = ()
        [<Erase>]
        member this.ariaRowIndexText
            with set (value: string) = ()
        [<Erase>]
        member this.ariaRowSpan
            with set (value: int) = ()
        [<Erase>]
        member this.ariaSelected
            with set (bool: bool) = ()
        [<Erase>]
        member this.ariaSetSize
            with set (value: int) = ()
        [<Erase>]
        member this.ariaSort
            with set (value: string) = ()
        [<Erase>]
        member this.ariaValueMax
            with set (value: string) = ()
        [<Erase>]
        member this.ariaValueMin
            with set (value: string) = ()
        [<Erase>]
        member this.ariaValueNow
            with set (value: string) = ()
        [<Erase>]
        member this.ariaValueText
            with set (value: string) = ()
