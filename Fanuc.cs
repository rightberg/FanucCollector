
using System.Reflection;

public struct ModeData
{
	public string mode;
	public string run_state;
	public string status;
	public string shutdowns;
	public string hight_speed;
	public string axis_motion;
	public string mstb;
	public string load_excess;

	public void Inicialize(ushort handle, string series)
	{
		mode = Fanuc.GetMode(handle, series);
		run_state = Fanuc.GetRunState(handle, series);
		status = Fanuc.GetStatus(handle, series);
		shutdowns = Fanuc.GetShutdowns(handle);
		hight_speed = Fanuc.GetHightSpeed(handle);
		axis_motion = Fanuc.GetAxisMotion(handle, series);
		mstb = Fanuc.GetMstb(handle, series);
		load_excess = Fanuc.GetLoadExcess(handle, series);
	}
}

public struct ProgramData
{
	public string frame;
	public int main_prog_number;
	public int sub_prog_number;
	public int parts_count;
	public int tool_number;
	public int frame_number;

	public void Inicialize(ushort handle)
	{
		frame = Fanuc.GetFrame(handle);
		main_prog_number = Fanuc.GetMainProgramNumber(handle);
		sub_prog_number = Fanuc.GetSubProgramNumber(handle);
		parts_count = Fanuc.GetPartsCount(handle);
		tool_number = Fanuc.GetPartsCount(handle);
		frame_number = Fanuc.GetPartsCount(handle);
	}
}

public struct AxesData
{
	public int feedrate;
	public int feed_override;
	public float jog_override;
	public int jog_speed;
	public float current_load;
	public float current_load_percent;
	public Dictionary<string, int> servo_loads;

	public void Inicialize(ushort handle)
	{
		feedrate = Fanuc.GetFeedRate(handle);
		feed_override = Fanuc.GetFeedOverride(handle);
		jog_override = Fanuc.GetJogOverride(handle);
		jog_speed = Fanuc.GetJogSpeed(handle);
		current_load = Fanuc.GetServoCurrentLoad(handle);
		current_load_percent = Fanuc.GetServoCurrentPercentLoad(handle);
		servo_loads = Fanuc.GetAllServoLoad(handle);
	}
}

public class Collector
{
	public ModeData mode_data = new();
	public ProgramData program_data = new();
	public AxesData axes_data = new();
}

public static class Fanuc
{
	public static Collector GetCollector(string id, ushort port, int timeout, string series = "0i-F")
	{
		Collector collector = new();
		short result;
		result = Focas1.cnc_allclibhndl3(id, port, timeout, out ushort handle);
		if (result != Focas1.EW_OK)
		{
			collector.mode_data.Inicialize(handle, series);
			collector.program_data.Inicialize(handle);
			collector.axes_data.Inicialize(handle);
		}
		return collector;
	}

	private static string GetCncErrorMessage(short error_code)
	{
		return error_code switch
		{
			-17 => "Protocol error ",
			-16 => "Socket error",
			-15 => "DLL file error",
			-11 => "Bus error",
			-10 => "System error (2)",
			-9 => "Communication error of HSSB",
			-8 => "Handle number error",
			-7 => "Version mismatch between the CNC/PMC and library",
			-6 => "Abnormal library state",
			-5 => "System error",
			-4 => "Shared RAM parity error",
			-3 => "FANUC drivers installation error",
			-2 => "Reset or stop request",
			-1 => "Busy",
			0 => "Normal termination",
			1 => "Error(function is not executed, or not available)",
			2 => "Error(data block length error, error of number of data)",
			3 => "Error(data number error)",
			4 => "Error(data attribute error)",
			5 => "Error(data error)",
			6 => "Error(no option)",
			7 => "Error(write protection)",
			8 => "Error(memory overflow)",
			9 => "Error(CNC parameter error)",
			10 => "Error(buffer empty/full)",
			11 => "Error(path number error)",
			12 => "Error(CNC mode error)",
			13 => "Error(CNC execution rejection)",
			14 => "Error(Data server error)",
			15 => "Error(alarm)",
			16 => "Error(stop)",
			17 => "Error(State of data protection)",
			_ => "Unknown error",
		};
	}

#region Тест

	public static string GetMainProgramName(ushort handle)
	{
		if (handle == 0)
			return "НЕТ ДОСТУПА";
		else
		{
			Focas1.ODBEXEPRG main_program = new();
			short result = Focas1.cnc_exeprgname(handle, main_program);
			if (result != Focas1.EW_OK)
				GetCncErrorMessage(result);
			return new string(main_program.name).Trim('\0');
		}
	}

	public static string GetSubProgramName(ushort handle)
	{
		if (handle == 0)
			return "НЕТ ДОСТУПА";
		else
		{
			Focas1.ODBPRO sub_program = new();
			short result = Focas1.cnc_rdprgnum(handle, sub_program);
			if (result != Focas1.EW_OK)
				return GetCncErrorMessage(result);
			if (sub_program.data != sub_program.mdata)
				return result.ToString();
			return "Нет данных саб-программы";
		}
	}

	public static void GetSpeed()
	{
		Focas1.ODBSPEED speed = new Focas1.ODBSPEED();
		short ret = Focas1.cnc_rdspeed(0, -1, speed);
		if (ret == Focas1.EW_OK)
		{
			Console.WriteLine("{0} = {1}", (char)speed.actf.name, speed.actf.data);
			Console.WriteLine("{0} = {1}", (char)speed.acts.name, speed.acts.data);
		}
	}

	public static double AbsolutePosition(ushort handle)
	{
		if (handle == 0)
			return 0;

		Focas1.ODBAXIS _axisPositionAbsolute = new Focas1.ODBAXIS();
		short result = Focas1.cnc_absolute2(handle, 88, 8, _axisPositionAbsolute);

		if (result != Focas1.EW_OK)
			return result;

		return _axisPositionAbsolute.data[0] / 100;
	}

	public static double RelativePosition(ushort handle)
	{
		if (handle == 0)
			return 0;

		Focas1.ODBAXIS _axisPositionRelative = new Focas1.ODBAXIS();
		short result = Focas1.cnc_relative2(handle, 88, 8, _axisPositionRelative);

		if (result != Focas1.EW_OK)
			return result;

		return _axisPositionRelative.data[0] / 100;
	}

	public static double MachinePosition(ushort handle)
	{
		if (handle == 0)
			return 0;

		Focas1.ODBAXIS _axisPositionMachine = new Focas1.ODBAXIS();
		short result = Focas1.cnc_machine(handle, 88, 8, _axisPositionMachine);

		if (result != Focas1.EW_OK)
			return result;

		return _axisPositionMachine.data[0] / 100;
	}

	public static double GetAllAxisAbsolutePositions(ushort handle)
	{
		if (handle == 0)
			return 0;

		try
		{
			Focas1.ODBAXIS _axisPositionMachine = new Focas1.ODBAXIS();
			short result = Focas1.cnc_absolute(handle, -1, 4 + 4 * Focas1.MAX_AXIS, _axisPositionMachine);

			if (result != Focas1.EW_OK)
				return result;

			for (int i = 0; i < Focas1.MAX_AXIS; i++)
			{
				Console.WriteLine(i.ToString() + " = " + _axisPositionMachine.data[i] / 100);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}

		return 0;
	}

	#endregion

	#region Режимы и статусы работы 

	//Текущий режим работы(AUT):
	public static string GetModeString(short mode, string series)
	{
		if (series == "0i-D" || series == "0i-F")
		{
			return mode switch
			{
				0 => "Полуавтомат (MDI)",
				1 => "Автоматический (MEM)",
				3 => "Редактирование (EDIT)",
				4 => "Ручная подача с помощью маховика (HND)",
				5 => "Ручная непрерывная подача (JOG)",
				6 => "Обучение JOG (Tech in JOG)",
				7 => "Обучение HND (Teach In Handle)",
				8 => "Ручные фиксированные перемещения (INC)",
				9 => "Выход в ноль (REF) ",
				10 => "Автоматический DNC (RMT)",
				_ => "Другой режим (Other)",
			};
		}
		return "Серия контроллера не поддерживается";
	}

	public static string GetMode(ushort handle, string series)
	{
		if (handle == 0)
			return "НЕТ ДОСТУПА";

		Focas1.ODBST mode = new();

		short result = Focas1.cnc_statinfo(handle, mode);

		if (result != Focas1.EW_OK)
			return GetCncErrorMessage(result);

		return GetModeString(mode.aut, series);
	}

	//Состояние автоматической операции (RUN)
	public static string GetRunStateString(short mode, string series)
	{
		if (series == "0i-D" || series == "0i-F")
		{
			return mode switch
			{
				0 => "Сброс (RES)",
				1 => "Останов автоматической работы (STOP)",
				2 => "Приостанов автоматической работы (HOLD) ",
				3 => "Выполнение УП (STRT)",
				4 => "Отвод и возврат инструмента (MSTR)",
				_ => "Неизвестное состояние",
			};
		}
		return "Серия контроллера не поддерживается";
	}

	public static string GetRunState(ushort handle, string series)
	{
		if (handle == 0)
			return "НЕТ ДОСТУПА";

		Focas1.ODBST run_state = new();

		short result = Focas1.cnc_statinfo(handle, run_state);

		if (result != Focas1.EW_OK)
			return GetCncErrorMessage(result);

		return GetRunStateString(run_state.run, series);
	}

	public static string GetStatusString(short status)
	{
		return status switch
		{
			0 => "**** (Отсутствие редактирования УП)",
			1 => "EDIT (Редактирование УП)",
			2 => "SeaRCH (Поиск данных)",
			3 => "OUTPUT (Вывод данных)",
			4 => "INPUT (Ввод данных)",
			5 => "COMPARE (Сравнение данных)",
			6 => "Label Skip (Пропуск метки)",
			7 => "ReSTaRt (Перезапуск программы)",
			8 => "Work ShiFT (Режим измерения величины смещения начала системы координат детали)",
			9 => "PTRR (Отвод или повторное позиционирование инструмента)",
			10 => "RVRS (Реверсирование)",
			11 => "RTRY (Повторный подвод)",
			12 => "RVED (Завершение реверсирования)",
			13 => "HANDLE (Перекрытие обработчика)",
			14 => "OFfSeT (Режим измерения величины коррекции инструмента по длине)",
			15 => "Work OFfSet (Режим измерения нулевой точки работы)",
			16 => "AICC (Режим контурного управления с СИИ)",
			17 => "MEmory-CHecK (Проверка памяти программ)",
			18 => "CusToMer's BoarD (Контроль платы заказчика)",
			19 => "SAVE (Сохранение данных измерения крутящего момента)",
			20 => "AI NANO (Нанопрограммное контурное управление с СИИ)",
			21 => "AI APC (Режим контурного управления с прогнозированием и СИИ)",
			22 => "Многоблочный расширенный контроль предварительного просмотра",
			23 => "AICC 2 (Высокоточное контурное управление с СИИ)",
			24 => "Высокоточное нанопрограммное контурное управление с СИИ",
			26 => "OFSX (Режим изменения активной величины коррекции инструмента по оси Х)",
			27 => "OFSZ (Режим изменения активной величины коррекции инструмента по оси Z)",
			28 => "WZR (Режим изменения смещения начала координат заготовки)",
			29 => "OFSY (Режим изменения величины активной коррекции инструмента по оси Y)",
			30 => "LEN (Режим изменения смещения длины/оси Х)",
			31 => "TOFS (Режим изменения коррекции инструмента)",
			32 => "RAD (Режим изменения величины коррекции на радиус инструмента)",
			39 => "TCP (Режим управления центром инструмента при обработке по 5 осям)",
			40 => "TWP (Режим наклонной рабочей плоскости)",
			41 => "TCP+TWP (Управление центром инструмента при 5-осевой обработке и наклонной плоскости обработки)",
			42 => "APC (Расширенный режим управления предварительным просмотром)",
			43 => "PRG-CHK (Проверка программы на высокой скорости)",
			44 => "APC (Расширенный режим управления предварительным просмотром)",
			45 => "S-TCP (Плавное управление центральной точкой инструмента)",
			46 => "AICC 2 (Контурное управление с СИИ 2)",
			59 => "ALLSAVE (Высокоскоростное управление программами: выполняется сохранение программ)",
			60 => "NOTSAVE (Высокоскоростное управление программами: статус 'программы не сохраняются')",
			_ => "Неизвестный статус",
		};
	}

	public static string GetStatus(ushort handle, string series)
	{
		if (handle == 0)
			return "НЕТ ДОСТУПА";

		Focas1.ODBST status = new();

		short result = Focas1.cnc_statinfo(handle, status);

		if (result != Focas1.EW_OK)
			return GetCncErrorMessage(result);

		return GetStatusString(status.edit);
	}

	public static string GetShutdowns(ushort handle)
	{
		byte[] buf = new byte[256];
		short block_count = 0;
		ushort block_length = (ushort)buf.Length;
		short frame_result = Focas1.cnc_rdexecprog(handle, ref block_length, out block_count, buf);
		if (frame_result != Focas1.EW_OK)
			return GetCncErrorMessage(frame_result);
		string frame = System.Text.Encoding.Default.GetString(buf, 0, block_length);

		foreach (string shutdown in new string[] { "M00", "M01", "G04" })
			if (frame.Contains(shutdown))
				return shutdown;

		return "No stop";
	}

	public static string GetHightSpeed(ushort handle)
	{
		byte[] buf = new byte[256];
		short block_count = 0;
		ushort block_length = (ushort)buf.Length;
		short frame_result = Focas1.cnc_rdexecprog(handle, ref block_length, out block_count, buf);
		if (frame_result != Focas1.EW_OK)
			return GetCncErrorMessage(frame_result);
		string frame = System.Text.Encoding.Default.GetString(buf, 0, block_length);

		if (frame.Contains("G00"))
			return "G00 активен";
		else
			return "G00 не активен";
	}

	private static string GetAxisMotionString(int motion, string series)
	{
		if (series == "0i-D" || series == "0i-F")
		{
			return motion switch
			{
				1 => "Останов автоматической работы (MTN)",
				2 => "Останов автоматической работы (DWL)",
				_ => "Другой режим (Others)",
			};
		}
		return "Другой режим (Others)";
	}

	public static string GetAxisMotion(ushort handle, string series)
	{
		if (handle == 0)
			return "НЕТ ДОСТУПА";

		Focas1.ODBST data = new();

		short result = Focas1.cnc_statinfo(handle, data);

		if (result != Focas1.EW_OK)
			return GetCncErrorMessage(result);

		return GetAxisMotionString(data.motion, series);
	}

	private static string GetAxisMstbString(int motion, string series)
	{
		if (series == "0i-D" || series == "0i-F")
		{
			return motion switch
			{
				2 => "Выполнение M,S,T функции (FIN)",
				_ => "Другой режим (Others)",
			};
		}
		return "Другой режим (Others)";
	}

	public static string GetMstb(ushort handle, string series)
	{
		if (handle == 0)
			return "НЕТ ДОСТУПА";

		Focas1.ODBST data = new();

		short result = Focas1.cnc_statinfo(handle, data);

		if (result != Focas1.EW_OK)
			return GetCncErrorMessage(result);

		return GetAxisMstbString(data.mstb, series);
	}

	private static string GetLoadExcessString(int motion, string series)
	{
		if (series == "0i-D" || series == "0i-F")
		{
			return motion switch
			{
				6 => "Превышение нагрузки на оси (AXES LOAD EXCESS)",
				9 => "Превышение нагрузки на шпиндель (SPINDLE LOAD EXCESS)",
				_ => "Отсутствие превышения (NO EXCESS)",
			};
		}
		return "Другой режим (Others)";
	}

	public static string GetLoadExcess(ushort handle, string series)
	{
		if (handle == 0)
			return "НЕТ ДОСТУПА";

		short result = Focas1.cnc_alarm2(handle, out int alarm);

		if (result != Focas1.EW_OK)
			return GetCncErrorMessage(result);


		return GetLoadExcessString(alarm, series);
	}

	#endregion

	#region Информация по УП, функции, счетчики, инструмент 

	/*   Информация по УП, функции, счетчики, инструмент 
	*   1 Номер УП MainPrgNum	
	*   2 Номер подпрограммы PrgNum  
	*   3 Содержимое кадра Frame  
	*   4 Счетчик деталей PartsCount  
	*   5 Номер инструмента Tool Number  
	*   6 Номер кадра Frame Num 
	*/

	public static int GetMainProgramNumber(ushort handle)
	{
		if (handle == 0)
			return 0;
		else
		{
			Focas1.ODBPRO program_number = new();
			short result = Focas1.cnc_rdprgnum(handle, program_number);
			if (result != Focas1.EW_OK)
				return result;
			return program_number.mdata;
		}
	}

	public static int GetSubProgramNumber(ushort handle)
	{
		if (handle == 0)
			return 0;
		else
		{
			Focas1.ODBPRO program_number = new();
			short result = Focas1.cnc_rdprgnum(handle, program_number);
			if (result != Focas1.EW_OK)
				return result;
			return program_number.data;
		}
	}

	public static string GetFrame(ushort handle)
	{
		if (handle == 0)
			return "НЕТ ДОСТУПА";
		else
		{
			byte[] buf = new byte[256];
			ushort block_length = (ushort)buf.Length;
			short result = Focas1.cnc_rdexecprog(handle, ref block_length, out short block_count, buf);
			if (result != Focas1.EW_OK)
				return GetCncErrorMessage(result);
			return System.Text.Encoding.Default.GetString(buf, 0, block_length);
		}
	}

	public static int GetPartsCount(ushort handle)
	{
		if (handle == 0)
			return 0;
		else
		{
			Focas1.IODBPSD_1 parts_count = new();
			short result = Focas1.cnc_rdparam(handle, 6771, 0, 4, parts_count);
			if (result != Focas1.EW_OK)
				return result;
			return parts_count.ldata;
		}
	}

	public static int GetToolNumber(ushort handle)
	{
		if (handle == 0)
			return 0;
		else
		{
			Focas1.IODBTLMNG tool_number = new();
			short data_num = 1;
			short result = Focas1.cnc_rdtool(handle, 1, ref data_num, tool_number);
			if (result != Focas1.EW_OK)
				return result;
			return tool_number.data1.T_code;
		}
	}

	public static int GetFrameNum(ushort handle)
	{
		if (handle == 0)
			return 0;
		else
		{
			Focas1.ODBSEQ frame_number = new();
			short result = Focas1.cnc_rdseqnum(handle, frame_number);
			if (result != Focas1.EW_OK)
				return result;
			return frame_number.data;
		}
	}

	#endregion

	#region Оси координат, скорости подачи

	private static string GetUnitString(short unit)
	{
		return unit switch
		{
			0 => "mm/min",
			1 => "inch/min",
			2 => "rpm",
			3 => "mm/rev",
			4 => "inch/rev",
			_ => "Нет данных по ед. изм.",
		};
	}

	public static string GetFeedUnit(ushort handle)
	{
		if (handle == 0)
			return "НЕТ ДОСТУПА";

		Focas1.ODBSPEED speed = new Focas1.ODBSPEED();
		short type = 0;
		short result = Focas1.cnc_rdspeed(handle, type, speed);
		if (result != Focas1.EW_OK)
			return GetCncErrorMessage(result);
		else
			return GetUnitString(speed.actf.unit);
	}

	public static string GetSpindleUnit(ushort handle)
	{
		if (handle == 0)
			return "НЕТ ДОСТУПА";

		Focas1.ODBSPEED speed = new Focas1.ODBSPEED();
		short type = 1;
		short result = Focas1.cnc_rdspeed(handle, type, speed);
		if (result != Focas1.EW_OK)
			return GetCncErrorMessage(result);
		else
			return GetUnitString(speed.acts.unit);
	}

	public static int GetFeedRate(ushort handle)
	{
		if (handle == 0)
			return 0;

		Focas1.ODBACT feedrate = new Focas1.ODBACT();
		short result = Focas1.cnc_actf(handle, feedrate);
		if (result != Focas1.EW_OK)
			return result;
		else
			return feedrate.data;
	}

	private static int GetFeedOverrideValue(short fov)
	{
		return fov switch
		{
			0 => 0,
			1 => 10,
			2 => 20,
			3 => 30,
			4 => 40,
			5 => 50,
			6 => 60,
			7 => 70,
			8 => 80,
			9 => 90,
			10 => 100,
			11 => 110,
			12 => 120,
			13 => 130,
			14 => 140,
			15 => 150,
			16 => 160,
			17 => 170,
			18 => 180,
			19 => 190,
			20 => 200,
			_ => 0,
		};
	}

	public static int GetFeedOverride(ushort handle)
	{
		if (handle == 0)
			return 0;

		Focas1.IODBSGNL fov = new();
		short result = Focas1.cnc_rdopnlsgnl(handle, 0x20, fov);
		if (result != Focas1.EW_OK)
			return result;
		else
			return GetFeedOverrideValue(fov.feed_ovrd);
	}

	private static float GetJogOverrideValue(short fov)
	{
		return fov switch
		{
			0 => 0,
			1 => 0.1f,
			2 => 0.14f,
			3 => 0.2f,
			4 => 0.27f,
			5 => 0.37f,
			6 => 0.52f,
			7 => 0.72f,
			8 => 1f,
			9 => 1.4f,
			10 => 2f,
			11 => 2.7f,
			12 => 3.7f,
			13 => 5.2f,
			14 => 7.2f,
			15 => 10f,
			16 => 14f,
			17 => 20f,
			18 => 27f,
			19 => 37f,
			20 => 52f,
			21 => 72f,
			22 => 100f,
			23 => 140f,
			24 => 200f,
			_ => 0,
		};
	}

	//short result = Focas1.cnc_rdparam(handle, 1401, 0, 8, jov);
	//bool jog_enabled = (jov.cdata & 0x01) != 0;
	public static float GetJogOverride(ushort handle)
	{
		if (handle == 0)
			return 0;

		Focas1.IODBSGNL fov = new();
		short result = Focas1.cnc_rdopnlsgnl(handle, 0x20, fov);
		if (result != Focas1.EW_OK)
			return result;
		else
			return GetJogOverrideValue(fov.jog_ovrd);
	}

	//1423 parametrer (jog feed)
	public static int GetJogSpeed(ushort handle)
	{
		short length = Focas1.MAX_AXIS;
		Focas1.ODBAXDT data = new();
		short result = Focas1.cnc_rdaxisdata(handle, 5, 5, 5, ref length, data);
		if (result != Focas1.EW_OK)
			return result;
		else
		{
			bool jog_enabled = (data.data1.flag & 0x02) != 0;
			if (jog_enabled)
				return data.data1.data;
			else
				return 0;
		}
	}

	public static Dictionary<string, int> GetAllServoLoad(ushort handle)
	{
		Dictionary<string, int> data = new();
		if (handle == 0)
			return data;

		short num = Focas1.MAX_AXIS;
		Focas1.ODBSVLOAD parametr = new();

		short result = Focas1.cnc_rdsvmeter(handle, ref num, parametr);
		if (result == Focas1.EW_OK)
		{
			Type type = parametr.GetType();
			foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
			{
				Focas1.LOADELM value = (Focas1.LOADELM)field.GetValue(parametr);
				data.Add(field.Name, value.data);
			}
		}
		return data;
	}

	// N : 2086 Word axis Rated current parameter 
	public static float GetServoCurrentLoad(ushort handle)
	{
		if (handle == 0)
			return 0;

		Focas1.IODBPSD_1 N = new();
		float load = 0;
		short param_result = Focas1.cnc_rdparam(handle, 2086, 0, 8, N);
		if (param_result != Focas1.EW_OK)
			return 0;

		Focas1.ODBAD parametr = new();
		short load_result = Focas1.cnc_adcnv(handle, 2, 2, parametr);
		if (load_result == Focas1.EW_OK)
			load = parametr.data * N.cdata / 6554;

		return load;
	}

	// N : 2086 Word axis Rated current parameter 
	// Max : 2165 Word axis Maximum amplifier current 
	// CNC_RDCURRENT
	public static float GetServoCurrentPercentLoad(ushort handle)
	{
		if (handle == 0)
			return 0;

		Focas1.IODBPSD_1 N = new();
		short сurrent_result = Focas1.cnc_rdparam(handle, 2086, 0, 8, N);
		if (сurrent_result != Focas1.EW_OK)
			return 0;

		Focas1.IODBPSD_1 Max = new();
		float load = 0;
		short param_result = Focas1.cnc_rdparam(handle, 2165, 0, 8, Max);
		if (param_result != Focas1.EW_OK)
			return 0;

		Focas1.ODBAD parametr = new();
		short load_result = Focas1.cnc_adcnv(handle, 2, 2, parametr);
		if (load_result == Focas1.EW_OK)
		{
			load = parametr.data * N.cdata / 6554;
			load = (load / Max.cdata) * 100;
		}

		return load;
	}
	#endregion

	#region Шпиндель

	public static int GetSpindleSpeed(ushort handle)
	{
		if (handle == 0)
			return 0;

		Focas1.ODBSPEED speed = new();
		short type = 0;
		short result = Focas1.cnc_rdspeed(handle, type, speed);
		if (result != Focas1.EW_OK)
			return result;
		else
			return speed.acts.data;
	}


	// 3799 #1  NDPs When an analog spindle is used, a position coder disconnection check is:
	public static string GetSpindleSpeedParameter(ushort handle)
	{
		if (handle == 0)
			return "НЕТ ДОСТУПА";

		Focas1.IODBPSD_1 parameter = new();
		short result = Focas1.cnc_rdparam(handle, 2086, 0, 8, parameter);
		if (result != Focas1.EW_OK)
			return GetCncErrorMessage(result);

		bool check_level = (parameter.cdata & 0x01) != 0;

		if (check_level)
			return "Проверка отключения позиционного кодера не производится";
		else
			return "Производится проверка отключения позиционного кодера";
	}


	public static int GetMotorSpeedSpindle(ushort handle)
	{
		if (handle == 0)
			return 0;

		Focas1.ODBSPLOAD speed = new();
		short num = Focas1.MAX_AXIS;
		short result = Focas1.cnc_rdspmeter(handle, 1, ref num, speed);
		if (result != Focas1.EW_OK)
			return result;
		else
			return speed.spload1.spspeed.data;
	}

	public static int GetSpindleLoad(ushort handle)
	{
		if (handle == 0)
			return 0;

		Focas1.ODBSPLOAD speed = new();
		short num = Focas1.MAX_AXIS;
		short result = Focas1.cnc_rdspmeter(handle, 0, ref num, speed);
		if (result != Focas1.EW_OK)
			return result;
		else
			return speed.spload1.spload.data;
	}

	// 3708#7 : #6  TSO During a threading or tapping cycle, the spindle override is: 
	public static int GetSpindleOveeride(ushort handle)
	{
		return 100;
	}
	#endregion

	#region Аварии, ошибки, сообщения 

	private static string GetAlarmString(int alarm, string series)
	{
		if (series == "0i-D" || series == "0i-F")
		{
			return alarm switch
			{
				3 => "Предупреждение P/S ",
				19 => "Ошибка PMC",
				_ => "Неизвестная ошибка",
			};
		}
		return "Другой режим (Others)";
	}

	public static string GetAlarm(ushort handle, string series)
	{
		if (handle == 0)
			return "НЕТ ДОСТУПА";

		short result = Focas1.cnc_alarm2(handle, out int alarm);

		if (result != Focas1.EW_OK)
			return GetCncErrorMessage(result);


		return GetAlarmString(alarm, series);
	}

	#endregion
}
