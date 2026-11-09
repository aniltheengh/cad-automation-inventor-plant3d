;;; att_tools.lsp - AutoLISP / Visual LISP attribute tools (AutoCAD, Plant 3D, any toolset)
;;; Load: APPLOAD (or drag the file into the drawing). Folder must be in TRUSTEDPATHS when SECURELOAD=1.
;;;
;;;  ATTLIST      list Block / Tag / Value of every attribute in the drawing to the text window + CSV
;;;  ATTSET       set TAG = VALUE on every insert of a block   (dynamic blocks handled)
;;;  ATTRENUM     renumber a tag sequentially by X then Y position (e.g. valve numbers V-101, V-102 ...)
;;;  (att:set-all "TITLEBLOCK" "REV" "B")  - same as ATTSET, callable from a .scr script
(vl-load-com)

(defun att:effective-name (obj)
  ;; dynamic blocks -> real name, not "*U12"
  (if (vlax-property-available-p obj 'EffectiveName)
    (vla-get-EffectiveName obj)
    (vla-get-Name obj)))

(defun att:all-inserts (/ ss i lst)
  ;; every INSERT with attributes, model + paper space
  (if (setq ss (ssget "_X" '((0 . "INSERT") (66 . 1))))
    (repeat (setq i (sslength ss))
      (setq lst (cons (vlax-ename->vla-object (ssname ss (setq i (1- i)))) lst))))
  lst)

(defun att:csv (s)
  (strcat "\"" (vl-string-subst "\"\"" "\"" s) "\""))

(defun c:ATTLIST (/ f path n)
  (setq path (strcat (getvar "DWGPREFIX") (vl-filename-base (getvar "DWGNAME")) "_att.csv")
        f    (open path "w")
        n    0)
  (write-line "Handle,Block,Tag,Value" f)
  (foreach blk (att:all-inserts)
    (foreach att (vlax-invoke blk 'GetAttributes)
      (write-line (strcat (att:csv (vla-get-Handle att)) ","
                          (att:csv (att:effective-name blk)) ","
                          (att:csv (vla-get-TagString att)) ","
                          (att:csv (vla-get-TextString att))) f)
      (setq n (1+ n))))
  (close f)
  (princ (strcat "\n" (itoa n) " attributes -> " path))
  (princ))

(defun att:set-all (bname tag val / n)
  (setq n 0)
  (foreach blk (att:all-inserts)
    (if (= (strcase (att:effective-name blk)) (strcase bname))
      (foreach att (vlax-invoke blk 'GetAttributes)
        (if (= (strcase (vla-get-TagString att)) (strcase tag))
          (progn (vla-put-TextString att val) (setq n (1+ n)))))))
  n)

(defun c:ATTSET (/ b tg v)
  (setq b  (getstring T "\nBlock name: ")
        tg (getstring "\nTag: ")
        v  (getstring T "\nNew value: "))
  (princ (strcat "\n" (itoa (att:set-all b tg v)) " attributes updated."))
  (princ))

(defun c:ATTRENUM (/ b tg pre start blks i)
  (setq b     (getstring T "\nBlock name: ")
        tg    (getstring "\nTag to renumber: ")
        pre   (getstring T "\nPrefix <V->: ")
        start (getint "\nStart number <101>: "))
  (if (= pre "") (setq pre "V-"))
  (if (null start) (setq start 101))
  (setq blks (vl-remove-if-not
               '(lambda (o) (= (strcase (att:effective-name o)) (strcase b)))
               (att:all-inserts)))
  ;; sort left->right, then top->bottom
  (setq blks (vl-sort blks
               '(lambda (a c / pa pc)
                  (setq pa (vlax-get a 'InsertionPoint) pc (vlax-get c 'InsertionPoint))
                  (if (equal (car pa) (car pc) 1e-6) (> (cadr pa) (cadr pc)) (< (car pa) (car pc))))))
  (setq i start)
  (foreach blk blks
    (foreach att (vlax-invoke blk 'GetAttributes)
      (if (= (strcase (vla-get-TagString att)) (strcase tg))
        (vla-put-TextString att (strcat pre (itoa i)))))
    (setq i (1+ i)))
  (princ (strcat "\n" (itoa (length blks)) " blocks renumbered."))
  (princ))

(princ "\natt_tools loaded: ATTLIST, ATTSET, ATTRENUM, (att:set-all blk tag val)")
(princ)
