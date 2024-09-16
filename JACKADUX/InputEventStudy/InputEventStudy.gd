extends Control

func _input(event):
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.is_pressed():
		print("_input, name:" + name)

func _gui_input(event):
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.is_pressed():
		print("_gui_input, name:" + name)

func _unhandled_input(event):
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.is_pressed():
		print("_unhandled_input, name:" + name)

func _process(delta):
	if Input.is_action_just_pressed("Click"):
		print("_process:" + name)
