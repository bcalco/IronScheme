#| License
Copyright (c) 2007-2016 Llewellyn Pritchard
All rights reserved.
This source code is subject to terms and conditions of the BSD License.
See docs/license.txt. |#

(library (ironscheme strings)
  (export 
    string-split
    string-index-of
    string-contains?
    string-starts-with?
    string-ends-with?
    string-ci-contains?
    string-ci-starts-with?
    string-ci-ends-with?    
    ;string-trim-start
    ;string-trim-end
    string-trim
    string-join
    string-replace
    )
  (import 
    (rnrs)
    (ironscheme contracts)
    (ironscheme clr))
    
  (define/contract (string-split str:string . del:string)
    (clr-call String (Split String[] StringSplitOptions) str (list->vector del) 'none))  
    
  (define/contract (string-join del:string strs:list)
    (clr-static-call String (Join String Object[]) del (list->vector strs)))  
    
  (define/contract (string-replace str:string old:string new:string)
    (if (string=? old "")
        str
        (clr-call String (Replace String String) str old new)))
          
  (define/contract string-index-of
    (case-lambda
      [(str:string sub:string)
        (clr-call String (IndexOf String) str sub)]
      [(str:string sub:string k:fixnum)  
        (clr-call String (IndexOf String Int32) str sub k)]))
    
  (define/contract (string-contains? str:string sub:string)
    (clr-call String (Contains String) str sub))
    
  (define/contract (string-ci-contains? str:string sub:string)
    (clr-call String (Contains String) (string-upcase str) (string-upcase sub)))

  (define/contract (string-starts-with? str:string sub:string)
    (clr-call String (StartsWith String) str sub))

  (define/contract (string-ci-starts-with? str:string sub:string)
    (clr-call String (StartsWith String) (string-upcase str) (string-upcase sub)))

  (define/contract (string-ends-with? str:string sub:string)
    (clr-call String (EndsWith String) str sub))

  (define/contract (string-ci-ends-with? str:string sub:string)
    (clr-call String (EndsWith String) (string-upcase str) (string-upcase sub)))

  (define/contract (string-trim str:string)
    (clr-call String Trim str)))
  