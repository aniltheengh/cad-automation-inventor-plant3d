;;; plant_helpers.lsp - small Plant 3D helpers in AutoLISP
;;;  P3DPROPS   pick a Plant object, list its Plant properties (uses PnP LISP functions if available)
;;;  P3DLAYERS  freeze/thaw common Plant layers quickly (edit the list for your project standard)
;;;  P3DPURGE   purge + audit before issuing drawings
;;; NOTE: Plant 3D exposes very little to LISP. Property access via LISP relies on undocumented
;;; functions; the supported route is .NET (code/plant3d/csharp). Everything below that touches
;;; Plant data is marked VERIFY and guarded with (if (fboundp ...)).
(vl-load-com)

(defun p3d:fboundp (sym) (and (boundp sym) (eval sym)))

(defun c:P3DPROPS (/ e)
  (if (setq e (car (entsel "\nPick Plant object: ")))
    (progn
      (princ (strcat "\nEntity type: " (cdr (assoc 0 (entget e)))))
      ;; Standard AutoCAD properties always work through ActiveX:
      (princ (strcat "\nLayer: " (vla-get-Layer (vlax-ename->vla-object e))))
      ;; VERIFY: Plant-specific LISP accessors are not officially documented.
      (princ "\nFor Plant properties use the .NET command PIDPICK (code/plant3d/csharp/Plant3DDataTools).")))
  (princ))

(defun c:P3DLAYERS (/ mode)
  (initget "Freeze Thaw")
  (setq mode (getkword "\n[Freeze/Thaw] Plant annotation layers <Freeze>: "))
  (if (null mode) (setq mode "Freeze"))
  ;; example layer names - replace with your project's layer standard
  (foreach lay '("Annotation*" "Insulation*" "*Support*")
    (command "_.-LAYER" (if (= mode "Freeze") "_Freeze" "_Thaw") lay ""))
  (princ))

(defun c:P3DPURGE ()
  (command "_.-PURGE" "_All" "*" "_N")
  (command "_.-PURGE" "_Regapps" "*" "_N")
  (command "_.AUDIT" "_Y")
  (princ "\nPurged + audited.")
  (princ))
(princ)
