    // src/components/Tile.tsx
import { Link } from "react-router-dom";

interface TileProps {
  title: string;
  description: string;
  to: string;
  visible: boolean;
}

const Tile: React.FC<TileProps> = ({ title, description, to, visible }) => {
  if (!visible) return null;

  return (
    <Link
      to={to}
      className="block rounded-xl border shadow-md p-4 hover:shadow-lg transition-all bg-white"
    >
      <h3 className="text-lg font-bold">{title}</h3>
      <p className="text-sm text-gray-600">{description}</p>
    </Link>
  );
};

export default Tile;

