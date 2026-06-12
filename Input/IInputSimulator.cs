namespace GenshinMacro.Input;

public interface IInputSimulator
{
    bool MoveMouseBy(int deltaX, int deltaY);
    bool RightClick();
    bool LeftButtonDown();
    bool LeftButtonUp();
    bool RightButtonDown();
    bool RightButtonUp();
}
