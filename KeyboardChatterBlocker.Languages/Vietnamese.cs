namespace KeyboardChatterBlocker.Languages;

public class Vietnamese : Language
{
	public Vietnamese()
	{
		language = "Tiếng Việt";
		chatter_threshold_label = "Ngưỡng mức nhận (ms)";
		ignored_async_keys = "Nút bỏ qua (Async)";
		ignored_keys = "Nút bỏ qua";
		listening_keys = "Đang nhận nút...";
		change_keys = "Đổi nút";
		clear_keys = "Xoá nút";
		enable_key_limiter = "Bật chế độ giới hạn nút";
		allowed_async_keys = "Nút nhận (Async)";
		allowed_keys = "Nút nhận";
		create_key_limiter_profile = "Tạo thư mục";
		delete_key_limiter_profile = "Xoá thư mục";
		key_limiter_profile_name = "Tên";
	}
}
