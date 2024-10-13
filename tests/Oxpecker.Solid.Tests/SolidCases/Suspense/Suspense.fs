module Oxpecker.Solid.Tests.Cases.Suspense

open Oxpecker.Solid
open Browser

[<SolidComponent>]
let Test () =
    let posts, _ = createResource(fun () -> promise { return [| "Post1"; "Post2" |]})
    let trivia, _ = createResource(fun () -> promise { return [| "Trivia1"; "Trivia2" |]})

    SuspenseList(revealOrder="forwards", tail="collapsed"){
        Portal(mount=document.getElementById("postsHeader")) { h2() { "Posts"} }
        Suspense(fallback=h2() { "Loading posts..." }){
            ul() {
                For(each=posts.current){
                    yield fun post _ -> li() { post }
                }
            }
        }
        Suspense(fallback=h2() { "Loading trivia..." }){
            ul() {
                For(each=trivia.current){
                    yield fun trivia _ -> li() { trivia }
                }
            }
        }
    }
